using KpCore;
using NuSMV;
using System;
using System.Collections.Generic;

namespace NuSMV
{
    internal class BInstances
    {
        /// <summary>
        /// For each MInstance (KP instance) generate a SMV instance.
        /// </summary>
        /// <param name="nuSMV"></param>
        /// <param name="module"></param>
        /// <param name="type"></param>
        /// <param name="kpInstance"></param>
        public static void generateSMVInstances(SMVModel nuSMV, Module module, MType type, MInstance kpInstance)
        {
            Instance smvInstance = null;
            //If module has division rule then being child or parent becomes matter, otherwise they are default SMV instances
            if (module.HasDivisionRule)
            {
                //Define module behaviour based on whether the instance is a parent or a child
                //Status parameter, parents start in ACTIVE; child instances start in NONEXIST state
                ParameterVar statusParam = new ParameterVar();

                //A Child instance
                if (kpInstance is KPChildInstance)
                {
                    // generate SMV Child Instance
                    smvInstance = new ChildInstance();
                    smvInstance.DivisionType = DIVISIONTYPE.CHILD;

                    //Start Status parameter in NONEXISTS state
                    statusParam.Name = CustomVariables.STATUS;
                    statusParam.Behaviour = VariableBehaviour.CUSTOM;
                    statusParam.Init = StatusStates.NONEXIST;
                    smvInstance.Parameters.Add(statusParam);
                    //add this instance to its parent children
                    crossReferChild2ParentInstance(nuSMV, type, kpInstance, smvInstance as ChildInstance);
                }
                // A Parent instance
                else
                {
                    // generate SMV Parent Instance
                    smvInstance = new ParentInstance();
                    smvInstance.DivisionType = DIVISIONTYPE.PARENT;

                    //Start Status parameter in Active state
                    statusParam.Name = CustomVariables.STATUS;
                    statusParam.Behaviour = VariableBehaviour.CUSTOM;
                    statusParam.Init = StatusStates.ACTIVE;
                    smvInstance.Parameters.Add(statusParam);
                }
                //set parameter to module as well.
                module.Parameters.Add(statusParam);
            }
            //Default instance
            else
            {
                smvInstance = new Instance();
                smvInstance.DivisionType = DIVISIONTYPE.NODIVISION;
            }
            smvInstance.Name = kpInstance.Name;
            //join custom parameters, e.g., status, with model parameters
            smvInstance.Parameters.UnionWith(getInstanceParameters(module, type, kpInstance));
            //cross reference module and its instance
            smvInstance.Module = module;
            module.Instance = smvInstance;
        }

        /// <summary>
        /// Cross reference child instance to its parent, and vice versa
        /// </summary>
        /// <param name="type"></param>
        /// <param name="kpInstance"></param>
        private static void crossReferChild2ParentInstance(SMVModel nuSMV, MType type, MInstance kpInstance, ChildInstance childSMVInstance)
        {
            //Access its parent KP instance
            MInstance parentKPInstance = (kpInstance as KPChildInstance).ParentKPInstance;
            ParentInstance parentSMVInstance = getSMVInstance(nuSMV, parentKPInstance);
            if (childSMVInstance.ParentInstance == null)
            {
                childSMVInstance.ParentInstance = parentSMVInstance;
            }
            else
            {
                throw new Exception("Error: Cross reference error with child and parent SMV instance...");
            }
            if (!parentSMVInstance.ChildInstances.Contains(childSMVInstance))
            {
                parentSMVInstance.ChildInstances.Add(childSMVInstance);
            }
        }

        /// <summary>
        /// Return SMV counterpart of given KP instance
        /// </summary>
        /// <param name="nuSMV"></param>
        /// <param name="parentKPInstance"></param>
        /// <returns></returns>
        private static ParentInstance getSMVInstance(SMVModel nuSMV, MInstance parentKPInstance)
        {
            ParentInstance parentSMVInstance = null;
            foreach (Module module in nuSMV.Modules)
            {
                if (module.Instance.Name.Equals(parentKPInstance.Name))
                {
                    if (module.Instance is ParentInstance)
                    {
                        parentSMVInstance = (ParentInstance)module.Instance;
                        break;
                    }
                }
            }
            if (parentSMVInstance == null)
            {
                throw new Exception("Error: KPInstance counterpart not found in any SMV modules!");
            }
            return parentSMVInstance;
        }

        /// <summary>
        /// Translates parameter of MInstances to set of ParameterVars. Adds sync variable as parameter to all instances,
        /// i.e., modules.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="type"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static HashSet<IVar> getInstanceParameters(Module module, MType type, MInstance instance)
        {
            HashSet<IVar> parameters = new HashSet<IVar>();
            Multiset ms = instance.Multiset;
            bool paramExist = ms.Count > 0 || module.HasConnection;
            if (paramExist)
            {
                foreach (var param in ms.Objects)
                {
                    ParameterVar parameter = new ParameterVar();
                    parameter.Name = param;
                    parameter.Init = ms[param].ToString();
                    //get instance parameters
                    parameters.Add(parameter);
                    //set module parameters.
                    module.Parameters.Add(parameter);
                }
            }
            //synch parameter
            ParameterVar synch = new ParameterVar();
            synch.Behaviour = VariableBehaviour.CUSTOM;
            synch.Name = CustomVariables.SYNCH;
            synch.Init = CustomVariables.SYNCH;
            synch.IsParamPrefixed = false;
            //get instance parameters
            parameters.Add(synch);
            //set module parameters.
            module.Parameters.Add(synch);

            return parameters;
        }

        /// <summary>
        /// Get list of all connected instances to @param kpInstance as the list of SMV instances.
        /// </summary>
        /// <param name="nuSMV"></param>
        /// <param name="kPsystem"></param>
        /// <param name="kpInstance"></param>
        /// <returns></returns>
        public static List<Instance> getInstanceConnections(SMVModel nuSMV, KPsystem kPsystem, MInstance kpInstance)
        {
            List<Instance> smvInstances = new List<Instance>();
            foreach (var connected in kpInstance.Connections)
            {
                MType targetType = findTargetTypeOfInstance(kPsystem, connected);
                Module targetModule = nuSMV.getModule(targetType, connected);
                Instance smvInstance = targetModule.Instance;
                if (smvInstance != null)
                {
                    // if connection is not exist then add it
                    if (!smvInstances.Exists(item => item.Name == smvInstance.Name))
                        smvInstances.Add(smvInstance);
                }
                else
                    throw new Exception("Corresponding SMV instance of " + connected.Name + " not found!");
            }

            return smvInstances;
        }

        /// <summary>
        /// Finds corresponding KP type of provided KP instance.
        /// </summary>
        /// <param name="kPsystem"></param>
        /// <param name="mInstance"></param>
        /// <returns></returns>
        private static MType findTargetTypeOfInstance(KPsystem kPsystem, MInstance mInstance)
        {
            MType targetType = null;
            foreach (var type in kPsystem.Types)
            {
                foreach (var instance in type.Instances)
                {
                    if (mInstance == instance)
                    {
                        targetType = type;
                        return targetType;
                    }
                }
            }
            return targetType;
        }
    }
}