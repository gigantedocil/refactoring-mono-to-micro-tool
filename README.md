# Microfactor

The main goal of this tool is to provide the means to automatically apply complex architectural refactorings with the objective of aiding in the transformation of a monolithic system into microservices.

This tool is part of the result of a master thesis entitled *Refactoring Monoliths into Microservices*. The thesis tries to establish a catalogue of refactoring patterns commonly used to transform monoliths into microservices which are derived from an analysis of the state of the art on this topic and some surveys to people with experience in the area, and uses the tool to test the viability of automating those refactorings. If you are interested about this topic you can read the entire published work [here](https://hdl.handle.net/10216/122620).

At the time being, only the *Local Method Method Call to Synchronous Remote Call*, which transforms a local call into a remote call with the use of HTTP, has been implemented.

## Remarks and Limitations

This tool was implemented targeting the .NET Core framework version 2.2 and developed in C#. It makes use of the Roslyn .NET Compiler 3 to get access to the project’s metadata and provides an API that can be used to perform static analysis of the documents that comprise the project and respective content by analysing their abstract syntax trees, semantic models and symbols.

The tool was implemented as a console application. Every interaction with the user is performed via a terminal. The user is prompted to answer questions related to the refactorings that he wishes to apply.

Although promising, due to time constraints the tool was only tested with a sample monolith (that is also shipped in the source of this repo) and in very simple scenarios.

Current limitations:

- The project must be a .NET Core project and have web dependencies installed (e.g.,
MVC).
- Only one class/interface is declared by file.
- There are no files with the same name.

## Implementation

As previously stated only The *Local Method Method Call to Synchronous Remote Call* refactoring is currently implemented. This refactoring has the base goal of extracting a method call into a separate independent microservice and making a bridge between the existing solution and the created microservice with the use of a synchronous remote protocol, which in this case will be HTTP due to its popularity and prominence in such systems.

The first step is to identify the method that should be extracted. This is done by asking the user to specify the path to the file and the name of the method. Using the Roslyn API, a document registry composed of an aggregate with the document’s name, path, syntax tree and semantic model is created and then used to search for the dependencies of that method, which are then marked and added to a Hashset. The method that gathers all the dependencies starts at the called method and then recursively traverses its dependencies by looking for relevant syntax tokens.

If an interface is found during the traversal of the documents, all of its implementations are sought after and traversed separately. A temporary data structure is used to progressively remove documents that have already been marked as a dependency in order to prevent circular dependencies, resource exhaustion or stack overflow’s from occurring.

After all the dependencies are found, a new web project is created programmatically with the aid of the .NET Core Command Line Interface (CLI). All the files that were marked as dependencies of the method are then copied into the new project and have their namespace tweaked to function properly in the new project. The bootstrap code contained in the Startup class is also copied into the new project but references to files that have not been copied are removed. The configurations inside this class are very important because they configure many general things about the application such as logging, dependency injection, security configurations, etc. A controller (REST resource representation) is created as a wrapper around the invoked method so that it can be called by the original project with an HTTP call.

Finally, on the original project an HTTP client is configured and added to the class that previously invoked the method. The invocation is altered to use the HTTP client instead of calling the method directly.

The tool was built to support the easy addition of more refactorings in the future. This is possible due to the use of a factory and the strategy design pattern. Each refactoring is a strategy, and a factory is used to decide at run time which strategy ought to be used. Because all strategies implement the same interface, the rest of the program can be written against the API declared by that interface and thus it is possible to take advantage of polymorphism.

## Testing

In order to test the tool and the refactoring, a sample monolith project was used to apply the refactoring and verify if the output, in this case, a new project with the correct files, were generated properly. The tests can be run as a regression battery in the future when new changes or refactorings are, respectively, introduced or added to make sure what already exists does not break with these new changes.

## User Interface

At the moment, the user interacts with the tool via a command line interface (Fig. 6.3). The terminal asks questions which the user must answer in order to proceed with the automatic application of the refactoring. Currently, the following questions must be answered in order to apply the refactoring:
- What is the solution path?
- What is the project name?
- What is the location of the file with the method that you wish to extract?
- What is the name of the method?
- What is the method occurrence number?
- What is the location where you want the project to be generated?
- This refactoring will make changes to your project. Please make sure you have a backup or
a restore point first. Do you want to proceed?

## Usage

In order to start using this tool you should:

- Clone this repo.
- Run the CodeAnalysisApp.MonolithDemo with IIS Express and perform an HTTP
GET request with an HTTP client (e.g., Postman) to the
http://localhost:23515/api/1/price endpoint and verify that it is returning 12.
- Stop the project.
- Run the project CodeAnalysisApp. A terminal will pop up and ask a series of
questions. Answer the questions with the default answers which appear inside parenthesis after each question.
- After the questions are answered the refactoring will be applied automatically.
- Run the monolith and the newly generated microservice and call the first endpoint again to verify that
it is still returning 12.
