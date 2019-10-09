# kPWorkbench

**kPWorkbench** is a software framework, developed to support the computational analysis of **kernel P systems**. 
The framework integrates a set of tools and translators that bridge several target specifications employed for kP system models, written in *kP-Lingua*. 
kPWorkbench permits **simulation** and **formal verification** of kP system models using several simulation and verification methodologies and tools. 

The framework features a native simulator, **kPWorkbench Simulator**, allowing the simulation of kP system models. In addition, it also integrates the **FLAME simulator**, a general purpose large scale agent based simulation environment, based on a method that allows to express kP systems as a set of communicating X-machines.  

kPWorkbench’s model checking environment permits the formal verification of kernel P system models. The framework supports both *Linear Temporal Logic (LTL)* and *Computation Tree Logic (CTL)* properties by making use of the **SPIN** and **NUSMV** model checkers. 
A property language is defined - **kP Queries**, comprising a list of natural language statements representing formal property patterns, from which the formal syntax of the SPIN and NUSMV formulas are automatically generated.
