using KpCore;
using KpCore.Ltl;
using KpUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KpLingua {
    public class KpLinguaReader {
        private string fileName;
        public string FileName { get { return fileName; } set { fileName = value; } }

        private KPsystem kp;
        private string kpl;
        private Dictionary<string, MInstance> instantiatedCompartments;
        private List<InstanceChain> instanceDeclarations;

        private List<string> errors;
        private List<string> warnings;
        private int line;

        public KpLinguaReader(string fileName) {
            FileName = fileName;
            errors = new List<string>();
            warnings = new List<string>();
            line = 1;
        }

        private string writeGuard(IGuard g) {
            if (g is BasicGuard) {
                BasicGuard bg = g as BasicGuard;
                string op = "_";
                switch (bg.Operator) {
                    case RelationalOperator.EQUAL: op = "="; break;
                    case RelationalOperator.GEQ: op = ">="; break;
                    case RelationalOperator.GT: op = ">"; break;
                    case RelationalOperator.LT: op = "<"; break;
                    case RelationalOperator.LEQ: op = "<="; break;
                    case RelationalOperator.NOT_EQUAL: op = "!"; break;
                }

                return op + " " + bg.Multiset.ToString();
            } else if (g is NegatedGuard) {
                return "! " + writeGuard((g as NegatedGuard).Operand);
            } else if (g is CompoundGuard) {
                CompoundGuard cg = g as CompoundGuard;
                return "(" + writeGuard(cg.Lhs) + (cg.Operator == BinaryGuardOperator.AND ? " & " : " | ") + writeGuard(cg.Rhs) + ")";
            }

            return "";
        }

        public unsafe KpModel Read() {
            kp = new KPsystem();
            KpModel kpModel = new KpModel(kp);

            if (instantiatedCompartments == null) {
                instantiatedCompartments = new Dictionary<string, MInstance>();
            } else {
                instantiatedCompartments.Clear();
            }

            if (instanceDeclarations == null) {
                instanceDeclarations = new List<InstanceChain>();
            } else {
                instanceDeclarations.Clear();
            }
            kpl = new StreamReader(fileName).ReadToEnd();

            fixed (char* x = kpl) {
                char* input = x;
                bool keepParsing = true;
                bool parsedSomething = false;
                while (keepParsing) {
                    input = skipSpaceAndComments(input);
                    if (*input == '\0') {
                        keepParsing = false;
                        if (!parsedSomething) {
                            throw new KplEmptyException();
                        }
                        continue;
                    }
                    ParseResult<Statement> statement = parseStatement(input, out input);
                    if (statement.Success) {
                        parsedSomething = true;
                        if (statement.Outcome.Type != null) {
                            //the lines are no longer necessary as whenver a type is parsed it is 
                            //created and registered with the KPsystem kp

                            //MType mt = statement.Outcome.Type;
                            //kp[mt.Name].Append(mt);
                        } else if (statement.Outcome.InstanceChain != null) {
                            instanceDeclarations.Add(statement.Outcome.InstanceChain);
                        }
                    } else {
                        throw new KplParseException(statement.Error);
                    }
                }

                //now make all the connections between declared instances
                foreach (InstanceChain id in instanceDeclarations) {
                    foreach (ChainElement e in id.Elements) {
                        if (e is TypedInstance) {
                            TypedInstance ti = e as TypedInstance;
                            kp[ti.MType].Instances.Add(ti.MInstance);
                            if (e.Value != null) {
                                instantiatedCompartments[e.Value] = ti.MInstance;
                            }
                        }
                    }
                }

                List<MInstance> prevList = new List<MInstance>();
                foreach (InstanceChain id in instanceDeclarations) {
                    MInstance prev = null;
                    prevList.Clear();
                    foreach (ChainElement e in id.Elements) {
                        MInstance mi = null;
                        if (e is TypedInstance) {
                            mi = (e as TypedInstance).MInstance;
                            if (prev != null) {
                                if (prev != mi) {
                                    prev.ConnectBidirectionalTo(mi);
                                }
                            } else if (prevList.Count > 0) {
                                foreach (MInstance instance in prevList) {
                                    if (instance != mi) {
                                        instance.ConnectBidirectionalTo(mi);
                                    }
                                }
                            }
                            prevList.Clear();
                            prev = mi;
                        } else if (e is TypedWildcard) {
                            List<MInstance> instanceList = kp[(e as TypedWildcard).MType].Instances;
                            if (prevList.Count > 0) {
                                foreach (MInstance instance1 in instanceList) {
                                    foreach (MInstance instance2 in prevList) {
                                        if (instance1 != instance2) {
                                            instance1.ConnectBidirectionalTo(instance2);
                                        }
                                    }
                                }
                            } else if (prev != null) {
                                foreach (MInstance instance in instanceList) {
                                    if (prev != instance) {
                                        prev.ConnectBidirectionalTo(instance);
                                    }
                                }
                            }
                            prevList.AddRange(instanceList);
                            prev = null;
                        } else {
                            if (e.Value != null) {
                                if (instantiatedCompartments.TryGetValue(e.Value, out mi)) {
                                    if (prev != null) {
                                        if (prev != mi) {
                                            prev.ConnectBidirectionalTo(mi);
                                        }
                                    } else if (prevList.Count > 0) {
                                        foreach (MInstance instance in prevList) {
                                            if (instance != mi) {
                                                instance.ConnectBidirectionalTo(mi);
                                            }
                                        }
                                    }
                                    prev = mi;
                                    prevList.Clear();
                                } else {
                                    throw new KplUnassignedVariableException(e.Value);
                                }
                            }
                        }
                    }
                }
            }

            return kpModel;
        }

        public unsafe void PrintErrors(TextWriter tw) {
            foreach (string error in errors) {
                tw.WriteLine(error);
            }
        }

        private unsafe bool isSpaceChar(char x) {
            return x == ' ' || x == '\r' || x == '\n' || x == '\t';
        }

        private unsafe bool isLetter(char x) {
            return (x >= 'A' && x <= 'Z') || (x >= 'a' && x <= 'z');
        }

        private unsafe bool isDigit(char x) {
            return x >= '0' && x <= '9';
        }

        private unsafe bool isAlphanumeric(char x) {
            return isDigit(x) || isLetter(x) || x == '_';
        }

        private unsafe bool isObjectChar(char x) {
            return isAlphanumeric(x) || x == '\'';
        }

        private unsafe char* skipSpaceAndComments(char* input) {
            char* p = input;
            p = skipSpace(p);
            char* q = p;
            q = skipComment(p);
            while (q != p) {
                q = skipSpace(q);
                p = q;
                q = skipComment(q);
            }

            return p;
        }

        private unsafe char* skipComment(char* input) {
            char* p = input;
            if (*p == '/') {
                if (*++p == '*') {
                    int l = line;
                    while (*++p != '\0') {
                        if (*p == '*') {
                            if (*++p == '/') {
                                line = l;
                                return ++p;
                            } else if (*p == '\0') {
                                return input;
                            }
                        }

                        if (*p == '\n') {
                            ++l;
                        }
                    }
                } else if (*p == '/') { //inline commment
                    do {
                        ++p;
                    } while (!(*p == '\n' || *p == '\0'));
                    if (*p != '\0') {
                        ++p;
                        line++;
                    }

                    return p;
                }
            }

            return input;
        }

        private unsafe char* skipSpace(char* input) {
            while (isSpaceChar(*input)) {
                if (*input == '\n') {
                    line++;
                }
                input++;
            }
            return input;
        }

        private unsafe char* skipTo(char* input, params char[] cs) {
            if (cs == null) {
                return input;
            }

            while (*input != '\0') {
                foreach (char c in cs) {
                    if (*input == c) {
                        return input;
                    } else {
                        input++;
                    }
                }
            }

            return input;
        }

        private unsafe ParseResult<string> parseKeyword(char* input, string keyword, out char* rest) {
            rest = input;
            int l = line;
            ParseResult<string> pr = new ParseResult<string>(false, keyword);
            pr.Error = "Expected string '" + keyword + "'. ";
            input = skipSpaceAndComments(input);
            fixed (char* kw = keyword) {
                char* k = kw;
                while (*input != '\0' && *k != '\0') {
                    if (*input != *k) {
                        pr.Error += "Found symbol '" + *input + "' at line " + line + ".";
                        line = l;
                        return pr;
                    }
                    input++;
                    k++;
                }
                if (*k != '\0') {
                    pr.Error = "'" + keyword + "' expected. End of input encoutered at line " + line + ".";
                    line = l;
                    return pr;
                }
                rest = input;
            }

            pr.Success = true;
            return pr;
        }

        private unsafe bool parseWord(string word, char* input, out char* rest, bool skip = true) {
            rest = input;
            int l = line;
            if (skip) {
                input = skipSpaceAndComments(input);
            }
            fixed (char* w = word) {
                char* k = w;
                while (*input != '\0' && *k != '\0') {
                    if (*input != *k) {
                        line = l;
                        return false;
                    }
                    ++input;
                    ++k;
                }

                if (*k != '\0') {
                    line = l;
                    return false;
                }
            }

            rest = input;
            return true;
        }

        private unsafe ParseResult<string> parseWildcard(char* input, out char* rest) {
            rest = input;
            int l = line;
            ParseResult<string> pr = new ParseResult<string>(false);
            pr.Error = "Wildcard expected. ";
            input = skipSpaceAndComments(input);
            if (*input == '*') {
                pr.Success = true;
                pr.Outcome = new String(input, 0, 1);
                rest = ++input;
            } else {
                if (*input == '\0') {
                    pr.Error += "End of input encoutered.";
                } else {
                    pr.Error += "Found symbol '" + *input + "' at line " + line + ".";
                }
                line = l;
            }

            return pr;
        }

        private unsafe ParseResult<string> parseIdentifier(char* input, out char* rest) {
            rest = input;
            ParseResult<string> pr = new ParseResult<string>(false, null);
            int l = line;
            input = skipSpaceAndComments(input);

            if (!isLetter(*input)) {
                pr.Error = "Identifier expected, found symbol '" + *input + "' at line " + line + ".";
                line = l;
                return pr;
            }

            char* r = input;
            int i = 0;
            do {
                input++;
                i++;
            } while (isAlphanumeric(*input));
            pr.Outcome = new String(r, 0, i);
            pr.Success = true;
            rest = input;

            return pr;
        }

        private unsafe ParseResult<Multiset> parsePossiblyBracedMultiset(char* input, out char* rest) {
            rest = input;

            ParseResult<Multiset> pr = parseMultiset(input, out input);
            if (pr.Success) {
                rest = input;
                return pr;
            } else {
                pr = parseBracedMultiset(input, out input);
                if (pr.Success) {
                    rest = input;
                    return pr;
                }
                return pr;
            }
        }

        private unsafe ParseResult<Multiset> parseBracedMultiset(char* input, out char* rest) {
            rest = input;
            int l = line;
            ParseResult<Multiset> pr = new ParseResult<Multiset>(false);

            input = skipSpaceAndComments(input);
            if (*input == '{') {
                input++;
                //first check if empty multiset is intended
                input = skipSpaceAndComments(input);
                if (*input == '}') {
                    pr.Outcome = new Multiset();
                    input++;
                    rest = input;
                    pr.Success = true;
                    return pr;
                }
                ParseResult<Multiset> ms = parseMultiset(input, out input);
                if (ms.Success) {
                    input = skipSpaceAndComments(input);
                    if (*input == '}') {
                        input++;
                        rest = input;
                        pr.Success = true;
                        pr.Outcome = ms.Outcome;
                        return pr;
                    } else {
                        if (*input == '\0') {
                            pr.Error = "'}' expected. End of input encountered at line " + line + ".";
                        } else {
                            pr.Error = "'}' expected. Found symbol '" + *input + "' at line " + line + ".";
                        }
                        line = l;
                        return pr;
                    }
                } else {
                    pr.Error = ms.Error;
                    line = l;
                    return pr;
                }
            } else {
                pr.Error = "'{' expected. Found symbol '" + *input + "' at line " + line + ".";
                line = l;
                return pr;
            }
        }

        private unsafe ParseResult<Multiset> parseMultiset(char* input, out char* rest) {
            rest = input;
            ParseResult<Multiset> pr = new ParseResult<Multiset>(false);
            int l = line;


            bool keepParsing = true;
            do {
                input = skipSpaceAndComments(input);
                char* r = input;

                int i = 0;
                while (isDigit(*input)) {
                    input++;
                    i++;
                }
                int m;
                if (i > 0) {
                    m = Int32.Parse(new String(r, 0, i));
                } else {
                    m = 1;
                }

                input = skipSpaceAndComments(input);
                int j = 0;
                r = input;
                while (isObjectChar(*input)) {
                    input++;
                    j++;
                }

                if (j > 0) {
                    string obj = new String(r, 0, j);
                    if (pr.Outcome == null) {
                        pr.Outcome = new Multiset(obj, m);
                    } else {
                        pr.Outcome.Add(obj, m);
                    }
                } else {
                    if (*input == '\0') {
                        pr.Error = "Invalid multiset declaration. Expected object label, end of input encountered at line " + line + ".";
                    } else {
                        pr.Error = "Invalid multiset declaration. Expected object label, found symbol '" + *input + "' at line " + line + ".";
                    }
                    line = l;
                    return pr;
                }

                input = skipSpaceAndComments(input);
                if (*input == ',') {
                    input++;
                } else {
                    keepParsing = false;
                }
            } while (keepParsing);

            if (pr.Outcome != null) {
                pr.Success = true;
                rest = input;
            }

            return pr;
        }

        private unsafe ParseResult<MultisetHybrid> parseTargetedMultiset(char* input, out char* rest) {
            rest = input;
            ParseResult<MultisetHybrid> pr = new ParseResult<MultisetHybrid>(false);
            int l = line;
            bool keepParsing = true;

            MultisetHybrid mh = new MultisetHybrid();
            Dictionary<IInstanceIdentifier, TargetedMultiset> tm = new Dictionary<IInstanceIdentifier, TargetedMultiset>();
            tm = mh.TargetedMultisets;
            Multiset main = mh.Multiset;
            Multiset current = new Multiset();

            bool expectcb = false;
            do {
                input = skipSpaceAndComments(input);
                if (!expectcb) {
                    if (*input == '{') {
                        input++;
                        expectcb = true;
                    }
                }
                char* r = input;

                int i = 0;
                while (isDigit(*input)) {
                    input++;
                    i++;
                }
                int m;
                if (i > 0) {
                    m = Int32.Parse(new String(r, 0, i));
                } else {
                    m = 1;
                }

                input = skipSpaceAndComments(input);
                int j = 0;
                r = input;
                while (isObjectChar(*input)) {
                    input++;
                    j++;
                }

                string obj = null;
                if (j > 0) {
                    obj = new String(r, 0, j);
                } else if (i > 0) {
                    if (*input == '\0') {
                        pr.Error = "Invalid multiset declaration. Expected object label, end of input encountered at line " + line + ".";
                    } else {
                        pr.Error = "Invalid multiset declaration. Expected object label, found symbol '" + *input + "' at line " + line + ".";
                    }
                    line = l;
                    return pr;
                }

                input = skipSpaceAndComments(input);
                if (*input == ',') {
                    if (obj == null) {
                        pr.Error = "Invalid multiset declarations. Expected object label, ',' encoutered at line " + line + ".";
                        line = l;
                        return pr;
                    } else {
                        if (expectcb) {
                            current.Add(obj, m);
                        } else {
                            main.Add(obj, m);
                        }
                        input++;
                    }
                } else if (*input == '}' && expectcb) {
                    input++;
                    input = skipSpaceAndComments(input);
                    if (*input == ',') {
                        if (obj != null) {
                            current.Add(obj, m);
                        }
                        main.Add(current);
                        input++;
                    } else {
                        if (obj != null) {
                            current.Add(obj, m);
                        }
                        ParseResult<string> type = parseTypeReference(input, out input);
                        if (type.Success) {
                            mh.Add(new TargetedMultiset(new InstanceIdentifier(type.Outcome), current));
                            input = skipSpaceAndComments(input);
                            if (*input == ',') {
                                input++;
                            } else {
                                keepParsing = false;
                            }
                        } else {
                            mh.Add(current);
                            keepParsing = false;
                        }
                    }
                    current = new Multiset();
                    expectcb = false;
                } else if (!expectcb) {
                    ParseResult<string> type = parseTypeReference(input, out input);
                    if (type.Success) {
                        mh.Add(new TargetedMultiset(new InstanceIdentifier(type.Outcome), new Multiset(obj, m)));
                        kp.EnsureType(type.Outcome);
                        input = skipSpaceAndComments(input);
                        if (*input == ',') {
                            input++;
                        } else {
                            keepParsing = false;
                        }
                    } else {
                        if (obj != null) {
                            main.Add(obj, m);
                        }
                        keepParsing = false;
                    }
                } else {
                    keepParsing = false;
                }
            } while (keepParsing);

            rest = input;
            pr.Outcome = mh;
            pr.Success = true;

            return pr;
        }

        private unsafe ParseResult<MType> parseType(char* input, out char* rest) {
            rest = input;
            int l = line;
            ParseResult<MType> mt = new ParseResult<MType>(false);
            ParseResult<string> pr = parseKeyword(input, "type", out input);
            if (pr.Success) {
                mt.IsPatternCongruous = true;
                ParseResult<string> x = parseIdentifier(input, out input);
                if (x.Success) {
                    pr = parseKeyword(input, "{", out input);
                    if (pr.Success) {
                        mt.Success = true;
                        MType mType = kp[x.Outcome];
                        mt.Outcome = mType;
                        ExecutionStrategy ex = mType.ExecutionStrategy;
                        bool isFirstEx = true;
                        bool keepParsing = true;
                        bool completeEnsemble = false;
                        StrategyOperator op = StrategyOperator.SEQUENCE;
                        while (keepParsing) {
                            StrategyOperator opp = parseOperator(input, out input);
                            if (opp == StrategyOperator.SEQUENCE) {
                                if (completeEnsemble) {
                                    op = opp;
                                    ex.Next = new ExecutionStrategy(op);
                                    ex = ex.Next;
                                    completeEnsemble = false;
                                }
                                isFirstEx = false;
                            } else {
                                op = opp;
                                completeEnsemble = false;
                                pr = parseKeyword(input, "{", out input);
                                if (!pr.Success) {
                                    keepParsing = false;
                                    mt.Error = pr.Error;
                                    mt.Success = false;
                                    return mt;
                                }
                                if (isFirstEx) {
                                    ex.Operator = op;
                                    isFirstEx = false;
                                } else {
                                    ex.Next = new ExecutionStrategy(op);
                                    ex = ex.Next;
                                }
                            }

                            pr = parseKeyword(input, "}", out input);
                            if (pr.Success) {
                                if (op == StrategyOperator.SEQUENCE) {
                                    keepParsing = false;
                                } else {
                                    completeEnsemble = true;
                                    pr = parseKeyword(input, "}", out input);
                                    if (pr.Success) {
                                        keepParsing = false;
                                    }
                                }
                            } else {
                                ParseResult<Rule> rr = parseRule(input, out input, mType.Name);
                                if (rr.Success) {
                                    ex.Rules.Add(rr.Outcome);
                                } else {
                                    keepParsing = false;
                                    mt.Error = rr.Error;
                                    mt.Success = false;
                                    line = l;
                                    return mt;
                                }
                            }
                        }
                        rest = input;
                        return mt;
                    } else {
                        mt.Error = pr.Error;
                    }
                } else {
                    mt.Error = x.Error;
                    return mt;
                }
            } else {
                mt.Error = pr.Error;
            }
            line = l;

            return mt;
        }

        private unsafe StrategyOperator parseOperator(char* input, out char* rest) {
            rest = input;
            StrategyOperator op = StrategyOperator.SEQUENCE;
            ParseResult<string> pr = parseKeyword(input, "choice", out rest);
            if (pr.Success) {
                op = StrategyOperator.CHOICE;
            } else {
                pr = parseKeyword(input, "max", out rest);
                if (pr.Success) {
                    op = StrategyOperator.MAX;
                } else {
                    pr = parseKeyword(input, "arbitrary", out rest);
                    if (pr.Success) {
                        op = StrategyOperator.ARBITRARY;
                    }
                }
            }

            return op;
        }

        private unsafe ParseResult<Rule> parseRule(char* input, out char* rest, string typeName = "default") {
            rest = input;
            int l = line;

            ParseResult<Rule> pr = new ParseResult<Rule>(false);
            Rule r = null;
            IGuard g = null;
            Multiset ms = null;

            ParseResult<IGuard> guard = parseGuard(input, out input);
            if (guard.Success) {
                g = guard.Outcome;
            }

            input = skipSpaceAndComments(input);
            ParseResult<Multiset> lhs = parsePossiblyBracedMultiset(input, out input);
            if (lhs.Success) {
                ms = lhs.Outcome;
            } else {
                pr.Error = lhs.Error;
                line = l;
                return pr;
            }

            bool keepParsing = false;
            input = skipSpaceAndComments(input);
            if (*input == '-') {
                input++;
                if (*input == '>') {
                    input++;
                    keepParsing = true;
                }
            }
            if (!keepParsing) {
                pr.Error = "Expected '->' operator. Found symbol '" + *input + "' at line " + line + ".";
                line = l;
                return pr;
            }

            input = skipSpaceAndComments(input);
            if (*input == '#') {
                r = new DissolutionRule(ms);
                input++;
            } else if (*input == '-') {
                input++;
                ParseResult<string> type = parseTypeReference(input, out input);
                if (type.Success) {
                    kp.EnsureType(type.Outcome);
                    r = LinkRule.LinkCreate(ms, new InstanceIdentifier(type.Outcome));
                } else {
                    input = skipSpaceAndComments(input);
                    if (*input == '.') {
                        //no type was intended
                        r = LinkRule.LinkCreate(ms, new InstanceIdentifier(typeName));
                    } else {
                        pr.Error = type.Error;
                        line = l;
                        return pr;
                    }
                }
            } else if (*input == '\\') {
                input++;
                if (*input == '-') {
                    input++;
                    ParseResult<string> type = parseTypeReference(input, out input);
                    if (type.Success) {
                        kp.EnsureType(type.Outcome);
                        r = LinkRule.LinkDestroy(ms, new InstanceIdentifier(type.Outcome));
                    } else {
                        input = skipSpaceAndComments(input);
                        if (*input == '.') {
                            //no type was intended
                            r = LinkRule.LinkDestroy(ms, new InstanceIdentifier(typeName));
                        } else {
                            pr.Error = type.Error;
                            line = l;
                            return pr;
                        }
                    }
                } else {
                    pr.Error = "Expected '-' after '\'. Found symbol " + *input + " at line " + line + ".";
                }
            } else if (*input == '[') {
                r = new DivisionRule(ms);
                DivisionRule dr = r as DivisionRule;
                input++;
                bool expectsb = true;
                while (keepParsing) {
                    input = skipSpaceAndComments(input);
                    if (expectsb) {
                        Multiset m = null;
                        if (*input == ']') {
                            m = new Multiset();
                            input++;
                        } else {
                            ParseResult<Multiset> cms = parseMultiset(input, out input);
                            if (cms.Success) {
                                m = cms.Outcome;
                                input = skipSpaceAndComments(input);
                                if (*input == ']') {
                                    input++;
                                } else {
                                    pr.Error = "']' expected. Found symbol '" + *input + " at line " + line + ".";
                                    line = l;
                                    return pr;
                                }
                            } else {
                                pr.Error = cms.Error;
                                line = l;
                                return pr;
                            }
                        }

                        ParseResult<string> type = parseTypeReference(input, out input);
                        if (type.Success) {
                            dr.Rhs.Add(new InstanceBlueprint(kp[type.Outcome], m));
                        } else {
                            dr.Rhs.Add(new InstanceBlueprint(kp[typeName], m));
                        }

                        expectsb = false;
                    } else {
                        if (*input == '[') {
                            input++;
                            expectsb = true;
                        } else {
                            keepParsing = false;
                        }
                    }
                }
            } else {
                ParseResult<MultisetHybrid> mh = parseTargetedMultiset(input, out input);
                if (mh.Success) {
                    if (mh.Outcome.TargetedMultisets.Count == 0) {
                        r = new RewritingRule();
                        RewritingRule rr = r as RewritingRule;
                        rr.Lhs.Add(ms);
                        rr.AddRhs(mh.Outcome.Multiset);
                    } else {
                        r = new RewriteCommunicationRule();
                        RewriteCommunicationRule rc = r as RewriteCommunicationRule;
                        rc.Lhs.Add(ms);
                        rc.AddRhs(mh.Outcome.Multiset);
                        rc.AddRhs(mh.Outcome.TargetedMultisets);
                    }
                } else {
                    pr.Error = mh.Error;
                    line = l;
                    return pr;
                }
            }

            if (r == null) {
                pr.Error = "Invalid rule declaration at line " + line + ".";
                line = l;
                return pr;
            }

            input = skipSpaceAndComments(input);
            if (*input == '.') {
                input++;
                rest = input;
                r.Guard = g;
                pr.Outcome = r;
                pr.Success = true;
                return pr;
            } else {
                pr.Error = "Unexpected symbol '" + *input + "' encoutered whilst parsing rule at line " + line + ".";
                line = l;
            }

            return pr;
        }

        private unsafe ParseResult<BasicGuard> parseBasicGuard(char* input, out char* rest) {
            rest = input;
            int l = line;

            ParseResult<BasicGuard> pr = new ParseResult<BasicGuard>(false);
            input = skipSpaceAndComments(input);

            RelationalOperator op = RelationalOperator.GEQ;
            if (*input == '=') {
                op = RelationalOperator.EQUAL;
                input++;
            } else if (*input == '!') {
                ++input;
                if (*input == '=') {
                    ++input;
                }
                op = RelationalOperator.NOT_EQUAL;
            } else if (*input == '>') {
                input++;
                if (*input == '=') {
                    op = RelationalOperator.GEQ;
                    input++;
                } else {
                    op = RelationalOperator.GT;
                }
            } else if (*input == '<') {
                input++;
                if (*input == '=') {
                    op = RelationalOperator.LEQ;
                    input++;
                } else {
                    op = RelationalOperator.LT;
                }
            } else {
                pr.Error = "Relational operator ('>', '<', '!', '=', '>=', '<=') expected. Found symbol '" + *input + "' at line " + line + ".";
                line = l;
                return pr;
            }

            ParseResult<Multiset> ms = parsePossiblyBracedMultiset(input, out input);
            if (ms.Success) {
                pr.Outcome = new BasicGuard(ms.Outcome, op);
                pr.Success = true;
                rest = input;
                return pr;
            } else {
                pr.Error = ms.Error;
                line = l;
                return pr;
            }
        }

        private unsafe ParseResult<IGuard> parseGuard(char* input, out char* rest, bool greedy = true, int precedence = 0) {
            rest = input;
            int l = line;

            ParseResult<IGuard> pr = new ParseResult<IGuard>(false);
            pr.Precedence = precedence;
            IGuard g = null;
            pr.KeepParsing = true;

            input = skipSpaceAndComments(input);
            ParseResult<BasicGuard> bg = parseBasicGuard(input, out input);
            if (bg.Success) {
                g = bg.Outcome;
            } else if (*input == '!') {
                input++;
                ParseResult<IGuard> negatedGuard = parseGuard(input, out input, false, precedence);
                if (negatedGuard.Success) {
                    g = new NegatedGuard(negatedGuard.Outcome);
                    pr.KeepParsing = negatedGuard.KeepParsing;
                } else {
                    pr.Error = negatedGuard.Error;
                    line = l;
                    return pr;
                }
            } else if (*input == '(') {
                input++;
                ParseResult<IGuard> eg = parseGuard(input, out input, true, precedence + 1);
                if (eg.Success) {
                    g = eg.Outcome;
                    pr.Precedence = precedence + 1;
                    input = skipSpaceAndComments(input);
                    if (*input == ')') {
                        input++;
                    } else {
                        if (*input == '\0') {
                            pr.Error = "')' expected. End of input encoutered at line " + line + ".";
                        } else {
                            pr.Error = "')' expected. Found symbol '" + *input + "' at line " + line + ".";
                        }
                        line = l;
                        return pr;
                    }
                } else {
                    pr.Error = eg.Error;
                    line = l;
                    return pr;
                }
            } else {
                pr.KeepParsing = false;
            }

            if (pr.KeepParsing && greedy) {
                input = skipSpaceAndComments(input);
                string x = new string(input);
                if (*input == ':') {
                    input++;
                    pr.KeepParsing = false;
                } else if (*input == '&') {
                    input++;
                    ParseResult<IGuard> rhs = parseGuard(input, out input, greedy, precedence);
                    if (rhs.Success) {
                        bool sw = false;
                        if (precedence == rhs.Precedence) {
                            if (rhs.Outcome is CompoundGuard) {
                                CompoundGuard cg = rhs.Outcome as CompoundGuard;
                                if (cg.Operator == BinaryGuardOperator.OR) {
                                    g = new CompoundGuard(BinaryGuardOperator.AND, g, cg.Lhs);
                                    cg.Lhs = g;
                                    g = cg;
                                    sw = true;
                                }
                            }
                        }

                        if (!sw) {
                            g = new CompoundGuard(BinaryGuardOperator.AND, g, rhs.Outcome);
                        }
                        pr.KeepParsing = rhs.KeepParsing;
                    } else {
                        pr.Error = rhs.Error;
                        line = l;
                        return pr;
                    }
                } else if (*input == '|') {
                    input++;
                    ParseResult<IGuard> rhs = parseGuard(input, out input, greedy, precedence);
                    if (rhs.Success) {
                        g = new CompoundGuard(BinaryGuardOperator.OR, g, rhs.Outcome);
                        pr.KeepParsing = rhs.KeepParsing;
                    } else {
                        pr.Error = rhs.Error;
                        line = l;
                        return pr;
                    }
                } else if (*input == ')') {
                    if (precedence == 0) {
                        pr.Error = "Unexpected symbol ')' encoutered at line  " + line + ".";
                        line = l;
                        return pr;
                    }
                    pr.KeepParsing = false;
                } else {
                    if (*input == '\0') {
                        pr.Error = "Invalid guard definition. End of input encoutered at line " + line + ".";
                    } else {
                        pr.Error = "Invalid guard definition. Unexpected symbol '" + *input + "' at line " + line + ".";
                    }
                    line = l;
                    return pr;
                }
            }

            if (g == null) {
                pr.Error = "Invalid guard definition at line " + line + ".";
                line = l;
            } else {
                rest = input;
                pr.Success = true;
                pr.Outcome = g;
            }

            return pr;
        }

        private unsafe ParseResult<string> parseTypeReference(char* input, out char* rest) {
            rest = input;
            int l = line;
            ParseResult<string> pr = new ParseResult<string>(false);
            input = skipSpaceAndComments(input);
            if (*input == '(') {
                input++;
                ParseResult<string> ident = parseIdentifier(input, out input);
                if (ident.Success) {
                    input = skipSpaceAndComments(input);
                    if (*input == ')') {
                        input++;
                        rest = input;
                        pr.Success = true;
                        pr.Outcome = ident.Outcome;
                        return pr;
                    } else {
                        if (*input == '\0') {
                            pr.Error = "')' expected. End of input encountered at line " + line + ".";
                        } else {
                            pr.Error = "')' expected. Found symbol '" + *input + "' at line " + line + ".";
                        }
                        line = l;
                        return pr;
                    }
                } else {
                    pr.Error = ident.Error;
                    line = l;
                    return pr;
                }
            } else {
                if (*input == '\0') {
                    pr.Error = "'(' expected. End of input encountered at line " + line + ".";
                } else {
                    pr.Error = "'(' expected. Found symbol '" + *input + "' at line " + line + ".";
                }
                line = l;
                return pr;
            }
        }

        private unsafe ParseResult<TypedInstance> parseInstance(char* input, out char* rest) {
            rest = input;
            int l = line;
            ParseResult<TypedInstance> ti = new ParseResult<TypedInstance>(false, new TypedInstance());
            ParseResult<string> ident = parseIdentifier(input, out input);
            MInstance instance = new MInstance();
            if (ident.Success) {
                instance.Name = ident.Outcome;
                ti.Outcome.Value = ident.Outcome;
            }

            ParseResult<Multiset> ms = parseBracedMultiset(input, out input);
            if (ms.Success) {
                ParseResult<string> tr = parseTypeReference(input, out input);
                if (tr.Success) {
                    instance.ReplaceMultiset(ms.Outcome);
                    ti.Outcome.MInstance = instance;
                    ti.Outcome.MType = tr.Outcome;
                    ti.Success = true;
                    rest = input;
                    return ti;
                } else {
                    ti.Error = tr.Error;
                    line = l;
                    return ti;
                }
            } else {
                ti.Error = ms.Error;
                line = l;
                return ti;
            }
        }

        private unsafe ParseResult<InstanceChain> parseInstanceChain(char* input, out char* rest) {
            rest = input;
            int l = line;

            ParseResult<InstanceChain> res = new ParseResult<InstanceChain>(false);
            bool keepParsing = true;
            InstanceChain chain = new InstanceChain();
            res.Outcome = chain;
            while (keepParsing) {
                input = skipSpaceAndComments(input);
                ParseResult<string> pr = parseWildcard(input, out input);
                if (pr.Success) {
                    ParseResult<string> tr = parseTypeReference(input, out input);
                    if (tr.Success) {
                        ChainElement e = new TypedWildcard(tr.Outcome, tr.Outcome);
                        chain.Elements.Add(e);
                    } else {
                        res.Error = tr.Error;
                        line = l;
                        return res;
                    }
                } else {
                    ParseResult<TypedInstance> pri = parseInstance(input, out input);
                    if (pri.Success) {
                        chain.Elements.Add(pri.Outcome);
                    } else {
                        pr = parseIdentifier(input, out input);
                        if (pr.Success) {
                            chain.Elements.Add(new ChainElement(pr.Outcome));
                        } else {
                            res.Error = pr.Error;
                            line = l;
                            return res;
                        }
                    }
                }

                input = skipSpaceAndComments(input);
                if (*input == '-') {
                    input++;
                    //keep parsing and adding to chain
                } else if (*input == '.') {
                    keepParsing = false;
                    input++;
                    rest = input;
                    res.Success = true;
                    return res;
                } else {
                    res.Error = "'-' or '.' expected. ";
                    if (*input == '\0') {
                        res.Error += "End of input encoutered at line " + line + ".";
                    } else {
                        res.Error += "Found symbol '" + *input + "' at line " + line + ".";
                    }
                    line = l;
                    return res;
                }
            }

            return res;
        }

        private unsafe ParseResult<Statement> parseStatement(char* input, out char* rest) {
            rest = input;
            int l = line;
            ParseResult<Statement> pr = new ParseResult<Statement>(false, new Statement());
            ParseResult<MType> tr = parseType(input, out input);
            if (tr.Success) {
                rest = input;
                pr.Success = true;
                pr.Outcome.Type = tr.Outcome;
                return pr;
            } else if (tr.IsPatternCongruous) {
                //if a type was intented then throw an error (type is a reserved keyword)
                pr.Error = "Invalid type definition. " + tr.Error;
                line = l;
                return pr;
            } else {
                ParseResult<InstanceChain> ic = parseInstanceChain(input, out input);
                if (ic.Success) {
                    pr.Outcome.InstanceChain = ic.Outcome;
                    rest = input;
                    pr.Success = true;
                    return pr;
                } else {
                    if (*input == '\0') {
                        pr.Error += "Type or instance definition expected. End of input encountered at line " + line + ".";
                    } else {
                        pr.Error = ic.Error;
                    }
                    line = l;
                    return pr;
                }
            }
        }

        private unsafe ParseResult<LtlProperty> parseLtlProperty(char* input, out char* rest) {
            rest = input;
            int l = line;

            input = skipSpaceAndComments(input);
            ParseResult<LtlProperty> pr = new ParseResult<LtlProperty>(false, null);
            ParseResult<string> spr = parseKeyword(input, "ltl", out input);
            
            if (spr.Success) {
                spr = parseIdentifier(input, out input);
                if (spr.Success) {
                    input = skipSpaceAndComments(input);
                    if (*input == '{') {
                        input++;

                        if (*input == '}') {
                            //success
                            pr.Success = true;
                            input++;
                            rest = input;
                        }
                    } else {

                    }
                } else {

                }
            } else {
                pr.Error = "Expected LTL expression preceeded by 'ltl' keyword.";
            }

            if (!pr.Success) {
                line = l;
            }

            return pr;
        }

        private unsafe ParseResult<LtlExpression> parseLtlExpression(char* input, out char* rest) {
            rest = input;
            int l = line;


            ParseResult<LtlExpression> pr = new ParseResult<LtlExpression>(false);

            input = skipSpaceAndComments(input);

            /*
             * ltlExpression = true | false | "(", ltlExpression, ")" | unaryLtlExpression | instanceStateExpression | binaryLtlExpression;
             */ 
            if (*input == '(') {
                input++;
                ParseResult<LtlExpression> exprPr = parseLtlExpression(input, out input);
                if (exprPr.Success) {
                    input = skipSpaceAndComments(input);
                    if (*input == ')') {
                        input++;
                        pr.Success = true;
                        pr.Outcome = exprPr.Outcome;
                    } else {
                        pr.Error = String.Format("Expected matching ')'. Found symbol {0} at line {1}.", *input, line);
                        pr.Success = false;
                    }
                } else {
                    pr.Error = exprPr.Error;
                }
            } else if (parseWord("true", input, out input)) {
                //pr.Outcome = new BoolLtlExpression(true);
                pr.Success = true;
            } else if (parseWord("false", input, out input)) {
                //pr.Outcome = new BoolLtlExpression(false);
                pr.Success = true;
            } else {
                ParseResult<UnaryLtlExpression> unaryLtl = parseUnaryLtlExpression(input, out input);
                if (unaryLtl.Success) {
                    pr.Success = true;
                    pr.Outcome = unaryLtl.Outcome;
                } else {
                    ParseResult<InstanceStateDecl> isDecl = parseInstanceStateExpression(input, out input);
                    if (isDecl.Success) {
                        pr.Success = true;
                        pr.Outcome = isDecl.Outcome;
                    }
                }
            }

            if (pr.Success) {
                BinaryLtlOperator op = BinaryLtlOperator.UNTIL;
                bool bin = false;
                input = skipSpaceAndComments(input);
                if (*input == 'U') {
                    ++input;
                    bin = true;
                } else if(*input == 'W') {
                    ++input;
                    op = BinaryLtlOperator.WEAK_UNTIL;
                    bin = true;
                } else if (*input == '&') {
                    ++input;
                    if (*input == '&') {
                        ++input;
                    }
                    op = BinaryLtlOperator.AND;
                    bin = true;
                } else if (*input == '|') {
                    ++input;
                    if (*input == '|') {
                        ++input;
                    }
                    op = BinaryLtlOperator.OR;
                    bin = true;
                } else if(*input == '-') {
                    ++input;
                    if (*input == '>') {
                        ++input;
                        op = BinaryLtlOperator.IMPLICATION;
                        bin = true;
                    } else {
                        pr.Success = false;
                        pr.Error = String.Format("Expected '>' after '-' (implication operator assumed). Found symbol '{0}' at line {1}.",
                            *input, line);
                    }
                } else if (*input == '<') {
                    ++input;
                    if (*input == '-') {
                        ++input;
                        if (*input == '>') {
                            ++input;
                            op = BinaryLtlOperator.EQUIVALENCE;
                            bin = true;
                        }
                    }

                    if (!bin) {
                        pr.Success = false;
                        pr.Error = String.Format("Expected equivalence operator '<->'. Found symbol '{0}' at line {1}.",
                            *input, line);
                    }
                }

                if (bin) {
                    ParseResult<LtlExpression> rhs = parseLtlExpression(input, out input);
                    if (rhs.Success) {

                    } else {
                        pr.Error = rhs.Error;
                    }
                }
            }

            if (pr.Success) {
                rest = input;
            } else {
                line = l;
            }

            return pr;
        }

        public unsafe ParseResult<UnaryLtlExpression> parseUnaryLtlExpression(char* input, out char* rest) {
            rest = input;
            int l = line;
            ParseResult<UnaryLtlExpression> pr = new ParseResult<UnaryLtlExpression>(false);

            input = skipSpaceAndComments(input);

            UnaryLtlOperator op = UnaryLtlOperator.ALWAYS;

            if (*input == 'G') {
                ++input;
                op = UnaryLtlOperator.ALWAYS;
                pr.Success = true;
            } else if (*input == 'F') {
                ++input;
                op = UnaryLtlOperator.EVENTUALLY;
                pr.Success = true;
            } else if (*input == '[') {
                ++input;
                if (*input == ']') {
                    ++input;
                    op = UnaryLtlOperator.ALWAYS;
                    pr.Success = true;
                } else {
                    pr.Error = "Expected ']' after '['. Found symbol '" + *input + "' at line " + line + ".";
                }
            } else if (*input == '<') {
                ++input;
                if (*input == '>') {
                    ++input;
                    op = UnaryLtlOperator.EVENTUALLY;
                    pr.Success = true;
                } else {
                    pr.Error = "Expected '>' after '<'. Found symbol '" + *input + "' at line " + line + ".";
                }
            } else if (*input == '!') {
                ++input;
                op = UnaryLtlOperator.NOT;
                pr.Success = true;
            }

            if (pr.Success) {
                input = skipSpaceAndComments(input);
                ParseResult<LtlExpression> prExpr = parseLtlExpression(input, out input);
                if (prExpr.Success) {
                    pr.Success = true;
                    pr.Outcome = new UnaryLtlExpression(op, prExpr.Outcome);
                } else {
                    pr.Success = false;
                }
            }

            if (pr.Success) {
                rest = input;
            } else {
                line = l;
            }

            return pr;
        }

        private unsafe ParseResult<InstanceStateDecl> parseInstanceStateExpression(char* input, out char* rest) {
            rest = input;
            int l = line;
            ParseResult<InstanceStateDecl> pr = new ParseResult<InstanceStateDecl>(false);

            input = skipSpaceAndComments(input);

            ParseResult<string> idPr = parseIdentifier(input, out input);
            if (idPr.Success) {
                input = skipSpaceAndComments(input);
                ParseResult<BasicGuard> gPr = parseBasicGuard(input, out input);
                if (gPr.Success) {
                    pr.Outcome = new InstanceStateDecl(idPr.Outcome, gPr.Outcome);
                    pr.Success = true;
                } else {
                    pr.Error = gPr.Error;
                }
            } else {
                pr.Error = idPr.Error;
            }

            if (pr.Success) {
                rest = input;
            } else {
                line = l;
            }

            return pr;
        }
    }

    public class InstanceStateDecl : LtlExpression {
        public string InstanceName { get; set; }
        public BasicGuard Guard { get; set; }

        public InstanceStateDecl() {

        }

        public InstanceStateDecl(string instanceName, BasicGuard bg) {
            InstanceName = instanceName;
            Guard = bg;
        }
    }

    public class TypedInstance : TypedElement {
        public MInstance MInstance { get; set; }

        public TypedInstance() {
        }

        public TypedInstance(MInstance instance, string mtype) {
            MInstance = instance;
            MType = mtype;
        }
    }

    public class TypedWildcard : TypedElement {
        public string Wildcard { get; set; }

        public TypedWildcard(string wildcard, string mtype) {
            Wildcard = wildcard;
            MType = mtype;
        }
    }

    public abstract class TypedElement : ChainElement {
        public string MType { get; set; }
    }

    public class InstanceChain {
        private List<ChainElement> elements;
        public List<ChainElement> Elements { get { return elements; } set { elements = value; } }

        public InstanceChain() {
            elements = new List<ChainElement>();
        }

        public InstanceChain Append(InstanceChain chain) {
            elements.AddRange(chain.Elements);
            return this;
        }
    }

    public class ChainElement {
        public string Value { get; set; }

        public ChainElement() {

        }
        public ChainElement(string val) {
            Value = val;
        }
    }

    public class Statement {
        public MType Type { get; set; }
        public InstanceChain InstanceChain { get; set; }
    }

    public class MultisetHybrid {
        private Multiset multiset;
        public Multiset Multiset { get { return multiset; } private set { multiset = value; } }

        private Dictionary<IInstanceIdentifier, TargetedMultiset> targetedMultisets;
        public Dictionary<IInstanceIdentifier, TargetedMultiset> TargetedMultisets { get { return targetedMultisets; } private set { targetedMultisets = value; } }

        public MultisetHybrid() {
            multiset = new Multiset();
            targetedMultisets = new Dictionary<IInstanceIdentifier, TargetedMultiset>();
        }

        public void Add(Multiset ms) {
            multiset.Add(ms);
        }

        public void Add(TargetedMultiset ms) {
            TargetedMultiset tm = null;
            targetedMultisets.TryGetValue(ms.Target, out tm);
            if (tm == null) {
                targetedMultisets.Add(ms.Target, ms);
            } else {
                tm.Multiset.Add(ms.Multiset);
            }
        }
    }

    public class ParseResult<T> {
        public bool Success { get; set; }
        public T Outcome { get; set; }
        public string Error { get; set; }
        public bool IsPatternCongruous { get; set; }

        public int Precedence { get; set; }
        public bool KeepParsing { get; set; }

        public ParseResult(bool success, T outcome)
            : this(success) {
            Outcome = outcome;
        }

        public ParseResult(bool success) {
            Success = success;
        }
    }

    public class KplParseException : Exception {

        public KplParseException(string msg)
            : base(msg) {
        }
    }

    public class KplEmptyException : KplParseException {
        public KplEmptyException()
            : base("Input source did not yield any tokens; there is nothing to parse!") {
        }
    }

    public class KplEndOfFileException : Exception {
        public KplEndOfFileException()
            : base("End of file reached.") {

        }
    }

    public class KplUnassignedVariableException : Exception {
        public KplUnassignedVariableException(string varName)
            : base("Variable '" + varName + "' is unassigned.") {

        }
    }
}
