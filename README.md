# kPWorkbench

## Table of Contents
- [Background](#background)
- [Download](#download)
- [Build](#build)
- [Usage](#usage)
- [License](#license)

## Background
**kPWorkbench** is a cross-platform software framework, developed to support the computational analysis of **kernel P systems**. 
The framework integrates a set of tools and translators that bridge several target specifications employed for kP system models, written in *kP-Lingua*. 
kPWorkbench permits **simulation** and **formal verification** of kP system models using several simulation and verification methodologies and tools. 

The framework features a native simulator, **kPWorkbench Simulator**, allowing the simulation of kP system models. In addition, it also integrates the **FLAME simulator**, a general purpose large scale agent based simulation environment, based on a method that allows to express kP systems as a set of communicating X-machines.  

kPWorkbench’s model checking environment permits the formal verification of kernel P system models. The framework supports both *Linear Temporal Logic (LTL)* and *Computation Tree Logic (CTL)* properties by making use of the **SPIN** and **NUSMV** model checkers. 
A property language is defined - **kP Queries**, comprising a list of natural language statements representing formal property patterns, from which the formal syntax of the SPIN and NUSMV formulas are automatically generated.

## Prerequisites
kPWorkbench is built on top of the .Net Core (3.0 or later) framework. Installation instructions for .Net Core and the different operating systems and platform types can be found [here](https://dotnet.microsoft.com/download/dotnet-core).

Moreover, performing end-to-end expriments using kPWorkbench requires the following tools to be installed and configured:
1. [gcc](https://gcc.gnu.org/install/binaries.html)
2. [Spin](http://spinroot.com/spin/Man/README.html#S2)
3. [NuSMV](http://nusmv.fbk.eu/NuSMV/download/getting_bin-v2.html)
4. [FLAME](http://flame.ac.uk/docs/install.html)

## Build
The kPWorkbench binaries for various platforms can be downloaded from the [releases](https://github.com/Kernel-P-Systems/kPWorkbench/releases) section.

## Build
### kPWorkbench
```sh
# cd into the kpw project directory
cd kPWorkbench/src/kpw/
```
Use .Net Core to build and publish the kPWorkbench command line tool binaries, by using the following command template:
```sh
# build and publish the kpw binaries for Linx
dotnet publish -r {target_os}-{target_architecture}
```

Examples of commonly-used build commands:
- Linux x64
```sh
# build and publish the kpw binaries for Linux x64
dotnet publish -r linux-x64
```
- OS X x64
```sh
# build and publish the kpw binaries for OS X x64
dotnet publish -r osx-x64
```
- Windows x64
```sh
# build and publish the kpw binaries for Windows x64
dotnet publish -r win-x64
```

### kPWorkbench UI
kPWorkbench UI is build on top of .Net Core, by taking advantage of the Windows Forms libraries. As of .Net Core 3.0, Windows Forms are only supported on Windows operating systems.  

In order to build to build and publish the kPWorkbench UI binaries, use the following commands:
```sh
# cd into the kPUI project directory
cd kPWorkbench/src/kPUI/
# build and publish the kPUI binaries for Windows x64
dotnet publish -r win-x64
```

## Usage
### kPWorkbench
After building the binaries of the kPWorkbench command line tool, more information on the various usage scenarios and parameters can be found by executing `kpw`, as below:
```
$ kpw
Kernel P system workbench v1.0.0.0
Usage:
        kpw [-action] <source_file> [Parameters] [-o <output_file>]

        Simulation:
        kpw -s <source_file>
                [Steps=(int > 0, default = 10)]
                [Seed=(int >= 0, default = 0)]
                [SkipSteps=(int >= 0, default = 0)]
                [RecordRuleSelection=(true|false, default=true)]
                [RecordTargetSelection=(true|false, default=true)]
                [RecordInstanceCreation=(true|false, default = true)]
                [RecordConfigurations=(true|false, default = true)]
                [ConfigurationsOnly=(true|false, default = false)]
        kpw -flame <source_file> <output_path>
        [-o <output_file>]

        Verification:
        kpw -spin <source_file> [-e <experiment_file>] [-o <output_file>]
        kpw -smv <source_file> [-e <experiment_file>] [-o <output_file>]
```

## License
[MIT](LICENSE)
