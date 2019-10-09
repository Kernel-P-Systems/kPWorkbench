using KpCore;
using KpExperiment.Model;
using KpLingua;
using NuSMV;
using KpSimulation;
using KpSpin;
using KpSpin.SpinVerificationModel;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using KpXML;
using KpFLAME;

namespace kpw
{
    class Program
    {
        static int Main(string[] args)
        {

            if (args == null || args.Length == 0 ||
                String.Compare(args[0], "usage", true) == 0 ||
                args[0] == "?" ||
                String.Compare(args[0], "help", true) == 0 ||
                String.Compare(args[0], "info", true) == 0)
            {
                PrintUsage();
                return 1;
            }

            int argIndex = 0;

            if (!args[argIndex].StartsWith("-") || args[argIndex] == "-s")
            {
                //simulation
                KpSimulationParams ksp = KpSimulationParams.Default();
                PropertyInfo[] kspProperties = typeof(KpSimulationParams).GetProperties();

                if (args[argIndex] == "-s")
                {
                    ++argIndex;
                }
                string srcFileName = args[argIndex];
                FileInfo fi = new FileInfo(srcFileName);
                if (!fi.Exists)
                {
                    Console.WriteLine("File '{0}' does not exist. Please specify a valid input file.", srcFileName);
                    return 1;
                }
                string outFileName = null;

                KpModel kpModel = null;
                try
                {
                    kpModel = KP.FromKpl(srcFileName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Parsing failed. Source: {0}", srcFileName);
                    Console.WriteLine(ex.Message);
                    return 1;
                }

                for (int i = ++argIndex; i < args.Length; i++)
                {
                    if (args[i] == "-o")
                    {
                        if (++i < args.Length)
                        {
                            outFileName = args[i];
                        }
                    }
                    else
                    {
                        string arg = args[i];
                        int eqIndex = arg.IndexOf('=');
                        if (eqIndex > 0)
                        {
                            string prop = arg.Substring(0, eqIndex);
                            string val = arg.Substring(eqIndex + 1);
                            bool propertyRecognized = false;
                            foreach (PropertyInfo kspProp in kspProperties)
                            {
                                if (kspProp.Name == prop)
                                {
                                    try
                                    {
                                        if (kspProp.PropertyType == typeof(Int32))
                                        {
                                            kspProp.SetValue(ksp, Int32.Parse(val));
                                        }
                                        else if (kspProp.PropertyType == typeof(bool))
                                        {
                                            kspProp.SetValue(ksp, bool.Parse(val));
                                        }
                                        else
                                        {
                                            kspProp.SetValue(ksp, val);
                                        }
                                    }
                                    catch
                                    {
                                        Console.WriteLine("*** Warning. Invalid format for property {0}: value must be of type {1}",
                                            kspProp.Name, kspProp.PropertyType.ToString());
                                    }
                                    propertyRecognized = true;
                                    break;
                                }
                            }

                            if (!propertyRecognized)
                            {
                                Console.WriteLine("*** Warning. Property {0} not recognized.", prop);
                            }
                        }
                    }
                }

                KpIdMap idMap = new KpIdMap(kpModel.KPsystem);

                KpSimulator simulator = new KpSimulator(ksp);
                //make sure newly created instances are accounted for in the idMap
                //for simulators, this must always be the first handler registered since it will assign
                //a valid Id to the instance, and id which is referred to in the Simulation interpreter
                simulator.RegisterNewInstance = idMap.RegisterNewInstance;

                TextWriter writer = outFileName == null ? Console.Out : new StreamWriter(outFileName);
                SimulationTextWriter textWriter = new SimulationTextWriter(writer);
                textWriter.BindToSimulator(simulator);

                simulator.Run(kpModel.KPsystem);

                return 0;
            }

            if (args[argIndex] == "-spin")
            {
                try
                {
                    //PerformPromelaTranslation(args);
                    PerformImprovedPromelaTranslation(args);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }

            if (args[argIndex] == "-smv")
            {
                try
                {
                    PerformNuSmvTransation(args);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }

            if (args[argIndex] == "-flame")
            {
                try
                {
                    PerformFlameTransation(args);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }

            Console.ReadKey();

            return 0;
        }

        private static void PerformPromelaTranslation(string[] args)
        {
            var kplFileName = string.Empty;
            var kpxFileName = string.Empty;
            var outputFileName = string.Empty;
            var translationParameters = PromelaTranslationParams.Default();
            var properties = typeof(PromelaTranslationConfig).GetProperties();
            var argIndex = 1;

            if (args.Length >= 2)
            {
                kplFileName = args[argIndex++];

                if (!new FileInfo(kplFileName).Exists)
                {
                    throw new Exception(string.Format("File '{0}' does not exist. Please specify a valid input file.", kplFileName));
                }
            }
            else
            {
                throw new Exception("Source file with kP system model not specified");
            }

            for (int i = argIndex; i < args.Length; i++)
            {
                if (args[i] == "-e")
                {
                    if (++i < args.Length)
                    {
                        kpxFileName = args[i];
                    }
                }
                else
                {
                    if (args[i] == "-o")
                    {
                        if (++i < args.Length)
                        {
                            outputFileName = args[i];
                        }
                    }
                    else
                    {
                        string arg = args[i];
                        int eqIndex = arg.IndexOf('=');
                        if (eqIndex > 0)
                        {
                            string prop = arg.Substring(0, eqIndex);
                            string val = arg.Substring(eqIndex + 1);
                            bool propertyRecognized = false;
                            foreach (PropertyInfo property in properties)
                            {
                                if (property.Name == prop)
                                {
                                    try
                                    {
                                        if (property.PropertyType == typeof(Int32))
                                        {
                                            property.SetValue(translationParameters, Int32.Parse(val));
                                        }
                                        else if (property.PropertyType == typeof(bool))
                                        {
                                            property.SetValue(translationParameters, bool.Parse(val));
                                        }
                                        else
                                        {
                                            property.SetValue(translationParameters, val);
                                        }
                                    }
                                    catch
                                    {
                                        throw new Exception(string.Format("*** Warning. Invalid format for property {0}: value must be of type {1}",
                                            property.Name, property.PropertyType.ToString()));
                                    }
                                    propertyRecognized = true;
                                    break;
                                }
                            }

                            if (!propertyRecognized)
                            {
                                throw new Exception(string.Format("*** Warning. Property {0} not recognized.", prop));
                            }
                        }
                    }
                }
            }

            KpModel kpModel = null;
            try
            {
                kpModel = KP.FromKpl(kplFileName);
            }
            catch (KplParseException kplException)
            {
                throw new Exception(string.Format("Failed to parse input file {0}. Reason: {1}", kplFileName, kplException.Message));
            }

            Experiment kpExperiment = null;
            if (!string.IsNullOrEmpty(kpxFileName))
            {
                if (new FileInfo(kpxFileName).Exists)
                {
                    try
                    {
                        kpExperiment = KP.FromKpx(kpxFileName);
                    }
                    catch (Exception exception)
                    {
                        throw new Exception(string.Format("Failed to parse input file {0}. Reason: {1}", kplFileName, exception.Message));
                    }
                }
                else
                {
                    throw new Exception(string.Format("File '{0}' does not exist. Please specify a valid experiment file.", kpxFileName));
                }
            }

            Console.WriteLine("Generating verification model {0} with: ", outputFileName);
            Console.WriteLine();
            foreach (PropertyInfo property in properties)
            {
                Console.WriteLine("{0} = {1}", property.Name, property.GetValue(translationParameters).ToString());
            }
            Console.WriteLine();

            var stream = string.IsNullOrEmpty(outputFileName) ? Console.Out : new StreamWriter(outputFileName);
            KP.WritePromela(kpModel.KPsystem, kpExperiment, translationParameters, stream);
        }

        private static int PerformImprovedPromelaTranslation(string[] args)
        {
            var argIndex = 1;

            if (args.Length < 2)
            {
                Console.WriteLine("Source file with kP system model not specified.");
                return 1;
            }

            string srcFileName = args[argIndex];
            FileInfo fi = new FileInfo(srcFileName);

            if (!fi.Exists)
            {
                Console.WriteLine("File '{0}' does not exist. Please specify a valid input file.", srcFileName);
                return 1;
            }

            string outFileName = string.Empty;
            string kpxFile = string.Empty;

            VerificationModelParams vmp = VerificationModelParams.Default();
            PropertyInfo[] vmpProperties = typeof(VerificationModelParams).GetProperties();

            for (int i = ++argIndex; i < args.Length; i++)
            {
                if (args[i] == "-o")
                {
                    if (++i < args.Length)
                    {
                        outFileName = args[i];
                    }
                }
                if (args[i] == "-e")
                {
                    if (++i < args.Length)
                    {
                        kpxFile = args[i];
                    }
                }
                else
                {
                    string arg = args[i];
                    int eqIndex = arg.IndexOf('=');
                    if (eqIndex > 0)
                    {
                        string prop = arg.Substring(0, eqIndex);
                        string val = arg.Substring(eqIndex + 1);
                        bool propertyRecognized = false;
                        foreach (PropertyInfo vmpProp in vmpProperties)
                        {
                            if (vmpProp.Name == prop)
                            {
                                try
                                {
                                    if (vmpProp.PropertyType == typeof(Int32))
                                    {
                                        vmpProp.SetValue(vmp, Int32.Parse(val));
                                    }
                                    else if (vmpProp.PropertyType == typeof(bool))
                                    {
                                        vmpProp.SetValue(vmp, bool.Parse(val));
                                    }
                                    else
                                    {
                                        vmpProp.SetValue(vmp, val);
                                    }
                                }
                                catch
                                {
                                    Console.WriteLine("*** Warning. Invalid format for property {0}: value must be of type {1}",
                                        vmpProp.Name, vmpProp.PropertyType.ToString());
                                }
                                propertyRecognized = true;
                                break;
                            }
                        }

                        if (!propertyRecognized)
                        {
                            Console.WriteLine("*** Warning. Property {0} not recognized.", prop);
                        }
                    }
                }
            }

            KpModel kpModel = null;
            try
            {
                kpModel = KP.FromKpl(srcFileName);
            }
            catch (KplParseException kplException)
            {
                Console.WriteLine("Parsing failed. Source: {0}", srcFileName);
                Console.WriteLine(kplException.Message);
                return 1;
            }

            Experiment kpExperiment = null;
            if (!string.IsNullOrEmpty(kpxFile))
            {
                try
                {
                    kpExperiment = KP.FromKpx(kpxFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Parsing failed. Source: {0}", kpxFile);
                    Console.WriteLine(ex.Message);
                    return 1;
                }
            }

            Console.WriteLine("Generating verification model {0} with: ", outFileName);
            Console.WriteLine();
            foreach (PropertyInfo vmpProp in vmpProperties)
            {
                Console.WriteLine("{0} = {1}", vmpProp.Name, vmpProp.GetValue(vmp).ToString());
            }
            Console.WriteLine();

            TextWriter owt = string.IsNullOrEmpty(outFileName) ? Console.Out : new StreamWriter(outFileName);
            KP.WriteVerificationPromelaModel(kpModel, kpExperiment, vmp, owt);
            //KP.WritePromela(kpModel.KPsystem, owt);

            return 0;
        }

        private static void PerformNuSmvTransation(string[] args)
        {
            var argIndex = 1;

            if (args.Length < 3)
            {
                throw new Exception("Source file with kP system model not specified.");
            }

            string kplFileName = args[argIndex];
            FileInfo fi = new FileInfo(kplFileName);
            if (!fi.Exists)
            {
                throw new Exception(string.Format("File '{0}' does not exist. Please specify a valid input file.", kplFileName));
            }

            string outFileName = null;
            string kpxFileName = null;

            for (int i = ++argIndex; i < args.Length; i++)
            {
                if (args[i] == "-e")
                {
                    if (++i < args.Length)
                    {
                        kpxFileName = args[i];
                    }
                }
                else if (args[i] == "-o")
                {
                    if (++i < args.Length)
                    {
                        outFileName = args[i];
                    }
                }
            }

            KpModel kpModel = null;
            try
            {
                Console.WriteLine("-- KP model is loading");
                kpModel = KP.FromKpl(kplFileName);
                Console.WriteLine("---- KP model loaded");
            }
            catch (KplParseException kplException)
            {
                throw new Exception(string.Format("Failed to parse input file {0}. Reason: {1}", kplFileName, kplException.Message));
            }

            Experiment kpExperiment = null;
            if (!string.IsNullOrEmpty(kpxFileName))
            {
                if (new FileInfo(kpxFileName).Exists)
                {
                    try
                    {
                        kpExperiment = KP.FromKpx(kpxFileName);
                    }
                    catch (Exception exception)
                    {
                        throw new Exception(string.Format("Failed to parse input file {0}. Reason: {1}", kplFileName, exception.Message));
                    }
                }
                else
                {
                    throw new Exception(string.Format("File '{0}' does not exist. Please specify a valid experiment file.", kpxFileName));
                }
            }

            Console.WriteLine("-- Translation to NuSMV is starting");
            TranslateSMV.Translate(kpModel, kpExperiment, outFileName);
            Console.WriteLine("---- KP model translated NuSMV model successfully.");
        }

        private static void PerformFlameTransation(string[] args)
        {
            var argIndex = 1;

            if (args.Length < 2)
            {
                throw new Exception("Source file with kP system model not specified.");
            }

            string kplFileName = args[argIndex];
            FileInfo fi = new FileInfo(kplFileName);
            if (!fi.Exists)
            {
                throw new Exception(string.Format("File '{0}' does not exist. Please specify a valid input file.", kplFileName));
            }

            string outPathName = args[2];
            if (!Directory.Exists(outPathName))
            {
                throw new Exception(string.Format("Path '{0}' does not exist. Please specify a valid path.", kplFileName));
            }

            KpModel kpModel = null;
            try
            {
                Console.WriteLine("-- KP model is loading");
                kpModel = KP.FromKpl(kplFileName);
                Console.WriteLine("---- KP model loaded");
            }
            catch (KplParseException kplException)
            {
                throw new Exception(string.Format("Failed to parse input file {0}. Reason: {1}", kplFileName, kplException.Message));
            }

            Console.WriteLine("-- Translation to FLAME is starting");
            KPsystemXMLWriter kPsystemXML = new KPsystemXMLWriter(kpModel.KPsystem);
            try
            {
                Directory.CreateDirectory(outPathName + @"\ite");
            }
            catch (KplParseException kplException)
            {
                throw new Exception(string.Format("Failed to parse create directory {0}. Reason: {1}", kplFileName, kplException.Message));
            }
            using (StreamWriter writer = new StreamWriter(outPathName + "model_generated.xml"))
            {
                writer.Write(kPsystemXML.ToXML());
            }
            kPsystemXML.SaveCFiles(outPathName);
            using (StreamWriter writer = new StreamWriter(outPathName + @"ite\0.xml"))
            {
                writer.Write(kPsystemXML.ToAgentsInitialConfiguration());
            }
            using (StreamWriter writer = new StreamWriter(outPathName + @"ite\map.txt"))
            {
                writer.Write(kPsystemXML.MapObjectIds);
            }
            Console.WriteLine("---- KP model translated FLAME model successfully.");
        }

        static void PrintUsage()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            Console.WriteLine("Kernel P system workbench v{0}", fvi.FileVersion);
            Console.WriteLine("Usage:");
            Console.WriteLine("\tkpw [-action] <source_file> [Parameters] [-o <output_file>]");
            Console.WriteLine();
            Console.WriteLine("\tSimulation: ");
            Console.WriteLine("\tkpw -s <source_file>");
            Console.WriteLine("\t\t[Steps=(int > 0, default = 10)]");
            Console.WriteLine("\t\t[Seed=(int >= 0, default = 0)]");
            Console.WriteLine("\t\t[SkipSteps=(int >= 0, default = 0)]");
            Console.WriteLine("\t\t[RecordRuleSelection=(true|false, default=true)]");
            Console.WriteLine("\t\t[RecordTargetSelection=(true|false, default=true)]");
            Console.WriteLine("\t\t[RecordInstanceCreation=(true|false, default = true)]");
            Console.WriteLine("\t\t[RecordConfigurations=(true|false, default = true)]");
            Console.WriteLine("\t\t[ConfigurationsOnly=(true|false, default = false)]");
            Console.WriteLine("\tkpw -flame <source_file> <output_path>");
            //Console.WriteLine("\t[-o[.(text|html, default=text)] <output_file> ...]");
            Console.WriteLine("\t[-o <output_file>]");
            Console.WriteLine();
            Console.WriteLine("\tVerification: ");
            Console.WriteLine("\tkpw -spin <source_file> [-e <experiment_file>] [-o <output_file>]");
            Console.WriteLine("\tkpw -smv <source_file> [-e <experiment_file>] [-o <output_file>]");
        }
    }
}
