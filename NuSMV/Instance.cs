using KpCore;
using System;
using System.Collections.Generic;

namespace NuSMV
{
    #region NUSMV Beans
    public enum DIVISIONTYPE
    {
        NODIVISION,
        PARENT,//if there is a division rule of this type then, this instance will be divided in future
        CHILD// /if there is a division rule of this type then, this instance will be generated from its parent
    }

    public class ChildInstance : Instance
    {
        public ChildInstance()
        {
            DivisionVariables = new HashSet<Variable>();
        }

        //Holds module status with and adds extra instance conditions.
        public Variable DivisionStatus { get; set; }

        //holds modules division variables with extra rules.
        public HashSet<Variable> DivisionVariables { get; set; }

        public Instance ParentInstance { get; set; }
    }

    public class Instance
    {
        private string name = "";

        public Instance()
        {
            Parameters = new HashSet<IVar>();
        }

        public List<Instance> ConnectedTo { get; set; }

        public DIVISIONTYPE DivisionType { get; set; }

        public Module Module { get; set; }

        public string Name
        {
            get { return name; }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    throw new Exception("Instance name cannot be empty.");
                }
                else if (SMVUtil.notReserved(value))
                {
                    name = value;
                }
                else
                    throw new Exception("The '" + value + "' is a reserved keyword in NUSMV. Rename it from the model.");
            }
        }

        public HashSet<IVar> Parameters { get; set; }

        public override string ToString()
        {
            return name + " : " + Module.Name;
        }
    }

    public class ParentInstance : Instance
    {
        private List<Instance> childInstances;

        public ParentInstance()
        {
        }

        public List<Instance> ChildInstances
        {
            get
            {
                if (childInstances == null)
                {
                    this.childInstances = new List<Instance>();
                    return this.childInstances;
                }
                else
                    return this.childInstances;
            }
            set { this.childInstances = value; }
        }
    }

    #endregion


    #region The items which extends from KP Models
    /// <summary>
    /// Copy of MInstance of KP System, in addition it specifies the instance
    /// is a child instance.
    /// </summary>
    public class KPChildInstance : MInstance
    {
        public KPChildInstance() { }
        public KPChildInstance(MInstance parentKPInstance)
        {
            ParentKPInstance = parentKPInstance;
            copyParentFeatures();
        }
        //Kind of Child ID, keep order of child.
        public int Order { get; set; }
        public MInstance ParentKPInstance { get; set; }
        public void copyParentFeatures() {
            this.multiset = ParentKPInstance.Multiset.Clone();
            PItem.CopyProperties(ParentKPInstance, this);
            LabelledPItem.CopyLabels(ParentKPInstance, this);
            this.Connections = ParentKPInstance.Connections;
            //add itself to connection list of target instances
            foreach (MInstance targetInstance in Connections)
            {
                if (!targetInstance.Connections.Contains(this))
                targetInstance.Connections.Add(this);

            }
        }
        public override string ToString()
        {
            return Name;
        }

    }

    #endregion
}