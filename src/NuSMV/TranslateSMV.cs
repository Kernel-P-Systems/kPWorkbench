using KpCore;
using KpExperiment.Model;
using KpExperiment.Verification.Translation;
using KpExperiment.Verification.Translation.Base;
using KpUtil;
using NuSMV;
using System;
using System.Linq;

namespace NuSMV
{
    /// <summary>
    /// 1. Translates KP Model to SMV model
    /// 2. Prints the SMV model to outFile
    /// 3. Calls PropertyTranslationManager to translate KPQ queries to LTL/CTL properties.
    /// </summary>
    public class TranslateSMV
    {
        public static void Translate(KpCore.KpModel kpModel, Experiment kpx, string outFileName)
        {
            KPsystem kpSystem = kpModel.KPsystem;
            KpMetaModel kpMetaModel = new KpMetaModel(kpSystem);

            try
            {
                //Translate KP model to SMV model
                SMVModel nuSMV = BNuSMV.buildModel(kpSystem);

                //SMVModels are loaded, in first run write variable to XML files, in 2nd run these values can be read.
                foreach (var module in nuSMV.Modules)
                {
                    BVariables.writeBounds2XML(module);
                }

                //Generate SMV file
                if (nuSMV != null)
                {
                    PrintNuSMV(nuSMV, kpMetaModel, kpx, outFileName);
                }
                else
                    throw new Exception("NuSMV translation failed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred during NuSMV translation.");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw ex;
            }
        }

        private static void PrintNuSMV(SMVModel nuSMV, KpMetaModel kpMetaModel, Experiment kpx, string outFileName)
        {
            //instantiate output file, otherwise to console
            Writer.FileName = outFileName;
            Writer.CleanFile(outFileName);
            bool firstModule = true;
            foreach (var module in nuSMV.Modules)
            {
                if (!firstModule)
                {
                    TUtil.AddDashedComment(module.Type.ToUpper());
                }
                WriteModule(module);
                WriteVariables(module);
                WriteINVAR(module);
                WriteInit(module);
                if (!module.HasDivisionRule && module.HasDissolutionRule)
                    WriteStatusNext(module);

                WriteNext(module);

                firstModule = false;
            }
            WriteMain(nuSMV);
            WriteStatusAndCommuncationNext(nuSMV);
            WriteProperties(kpx, kpMetaModel);
        }

        private static void WriteStatusNext(Module module)
        {
            string op = "";
            if (!module.HasDivisionRule)
                op += TVariables.VariableNext(module.Status);
            Writer.WriteLine(op);
        }

        private static void WriteStatusAndCommuncationNext(SMVModel nuSMV)
        {
            string op = "";
            //write instances
            foreach (var module in nuSMV.Modules)
            {
                Instance instance = module.Instance;
                //write parent instances...
                if (instance is ParentInstance)
                {
                    op += TVariables.WriteStatusNextWithInstance(module, instance);
                    op += TVariables.WriteParentDivisionVariableNext(module, instance as ParentInstance);
                }
                else if (instance is ChildInstance)
                {
                    //Print status rules of each child instance
                    op += TVariables.WriteChildStatusNext(module, instance as ChildInstance);
                    op += TVariables.WriteChildDivisionVariablesNext(instance as ChildInstance);
                }

                foreach (var variable in module.Variables)
                {
                    if (variable.Behaviour == VariableBehaviour.COMMUNICATION)
                        op += TVariables.CommunicationVariableNext(module, instance, variable);
                }
            }
            Writer.WriteLine(op);
        }

        private static void WriteMain(SMVModel nuSMV)
        {
            TUtil.AddDashedComment("MAIN");
            string op = SMVKeys.MODULE + " " + SMVKeys.MAIN + "\n";
            op += SMVKeys.VAR + "\n";
            //write synch,and PStep vars at first.
            op += TVariables.VariableDefinition(nuSMV.MainModule.Synch);
            op += TVariables.VariableDefinition(nuSMV.MainModule.PStep);

            //write instances
            foreach (var module in nuSMV.Modules)
            {
                //parent instance
                op += TVariables.Instances(module.Instance);
                //if exists, then child instances
                if (module.HasDivisionRule)
                {
                    foreach (var childInstance in module.ChildInstances)
                    {
                        op += TVariables.Instances(childInstance);
                    }
                }
            }
            op += SMVKeys.ASSIGN + "\n";

            //write init of synch and PStep vars
            op += TVariables.InitVariable(nuSMV.MainModule.Synch);
            op += TVariables.InitVariable(nuSMV.MainModule.PStep);

            //write next of synch and PStep vars
            op += TVariables.VariableNext(nuSMV.MainModule.Synch);
            op += TVariables.VariableNext(nuSMV.MainModule.PStep);

            Writer.WriteLine(op);
        }

        private static void WriteInit(Module module)
        {
            string op = SMVKeys.ASSIGN + "\n";
            if (module.HasDivisionRule || module.HasDissolutionRule)
            {
                op += TVariables.InitVariable(module.Status);
            }
            op += TVariables.InitVariable(module.Turn);
            if (module.HasArbitraryStrategy || module.HasSequenceStrategy || module.HasMaxStrategy)
            {
                op += TVariables.InitVariable(module.Count);
            }
            //execution strategy variables, e.g. choice
            int strategyIndex = 0;
            foreach (var strategy in module.ExecutionStrategies)
            {
                foreach (var variable in strategy.CustomVars)
                {
                    op += TVariables.InitVariable(variable);
                }
                strategyIndex++;
            }
            // model variables
            foreach (var variable in module.Variables)
            {
                op += TVariables.InitVariable(variable);
            }
            Writer.WriteLine(op);
        }

        private static void WriteINVAR(Module module)
        {
            string op = "";
            foreach (var strategy in module.ExecutionStrategies)
            {
                foreach (var variable in strategy.CustomVars)
                {
                    if (variable is NonDeterVar)
                    {
                        if (!string.IsNullOrWhiteSpace((variable as NonDeterVar).INVARCase.ToString()))
                            op += (variable as NonDeterVar).printINVAR() + ";\n";
                    }
                }
            }
            Writer.WriteLine(op);
        }

        private static void WriteModule(Module module)
        {
            string op = "";
            op += SMVKeys.MODULE + " " + module.Name;
            //add parameter
            op += " (";
            op += String.Join(", ", module.Parameters);
            op += " )";
            Writer.WriteLine(op);
        }

        private static void WriteNext(Module module)
        {
            string op = "";
            //write turn
            op += TVariables.VariableNext(module.Turn);
            //if there is arb, seq or max strategy
            if (module.HasArbitraryStrategy || module.HasSequenceStrategy || module.HasMaxStrategy)
            {
                op += TVariables.VariableNext(module.Count);
            }
            //execution strategy variables, e.g. choice
            foreach (var strategy in module.ExecutionStrategies)
            {
                foreach (var variable in strategy.CustomVars)
                {
                    op += TVariables.VariableNext(variable);
                }
            }
            // model variables
            foreach (var variable in module.Variables)
            {
                if (variable.Behaviour == VariableBehaviour.REWRITING)
                    op += TVariables.VariableNext(variable);
            }
            Writer.WriteLine(op);
        }

        private static void WriteVariables(Module module)
        {
            string op = SMVKeys.VAR + "\n";
            //define custom variables
            //status variable for all types
            op += TVariables.VariableDefinition(module.Status);
            op += TVariables.VariableDefinition(module.Turn);
            //if module has arb, seq or max strategies
            if (module.HasArbitraryStrategy || module.HasSequenceStrategy || module.HasMaxStrategy)
            {
                op += TVariables.VariableDefinition(module.Count);
            }
            if (module.HasConnection)
            {
                foreach (var connection in module.Connections)
                {
                    op += TVariables.VariableDefinition(connection);
                }
            }
            //execution strategy variables, e.g. choice
            foreach (var strategy in module.ExecutionStrategies)
            {
                foreach (var variable in strategy.CustomVars)
                {
                    op += TVariables.VariableDefinition(variable);
                }
            }
            // model variables
            //sort variables, then print
            module.Variables = SMVUtil.sortVariable(module.Variables);
            foreach (var variable in module.Variables)
            {
                op += "\t" + TVariables.VariableDefinition(variable);
            }
            Writer.WriteLine(op);
        }

        private static void WriteProperties(Experiment kpx, KpMetaModel kpMetaModel)
        {
            if (kpx != null)
            {
                if (kpx.LtlProperties != null && kpx.LtlProperties.Count() > 0)
                {
                    Writer.WriteLine("-- LTL Properties");
                    foreach (var ltlProperty in kpx.LtlProperties)
                    {
                        Writer.WriteLine(string.Format("LTLSPEC {0}", PropertyTranslationManager.Instance.Translate(ltlProperty, kpMetaModel, ModelCheckingTarget.NuSmv)));
                    }

                    Writer.Write(Environment.NewLine);
                }

                if (kpx.CtlProperties != null && kpx.CtlProperties.Count() > 0)
                {
                    Writer.WriteLine("-- CTL Properties");
                    foreach (var ctlProperty in kpx.CtlProperties)
                    {
                        Writer.WriteLine(string.Format("SPEC {0}", PropertyTranslationManager.Instance.Translate(ctlProperty, kpMetaModel, ModelCheckingTarget.NuSmv)));
                    }
                }
            }
        }
    }
}