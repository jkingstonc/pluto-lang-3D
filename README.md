# Pluto!

<p align="center">
![](https://i.imgur.com/Dvm9Eyh.png =250x250)
</p>


# What is Pluto?

Pluto is an interpreted scripting language, for indended use in C# runtime enviroments. It was originaly made for use in Unity3D games, for in game scripting [see Pluto3D]

# Runtime Compilation Design
As pluto was designed to be compiled on-the-fly, the language supports build in compilation from within a script. Say we want to compile different code depending on a variable, in Pluto we can do that!

	lang::compile(
		"var x = 123"
	)
	io::print(x)
	
# Native C# code
Pluto provides an easy to use method to provide native interfaces to C# code via script-accessable calls. This allows the language to be used in cases where a specificly taylored solution is desired for a task

# Examples
### Hello World
Pluto is focuses around namespaces, the *print* function is located in the *io* namespace

	using namespace io
	print("Hello world!")

### Modules
Pluto supports file importing, which are referenced as modules

	import "root.sub.file"
	import "another_root.sub"

### Namespaces
Pluto is focued around namespaces, this allows for reuse of variable names, and more modular programming

	namespace example
	{
		var x = "example"
	}
	io::print(example::x)
### Comments
Currently, Pluto only allows for single-line comments

	# this is a comment!

### Variables
Variables are not typed: they can take literally any value, even a class decleration

	var x = 99
	x = "now a string!"

### Exceptions
In Pluto, we can throw exceptions by calling the interrupt command

	class Test extends Exception
	{
		func Test
		{
			super.Exception("Test exception message")
		}
	}
	
	try
		interrupt Test()
	catch(exception)
		io::print(exception.message)

### Conditional Branching
Inside these condition blocks, we dont have to use curly braces for single line expressions

	if ("hi" == 2 or x != 5)
		var string = "go here"
	elif(False)
		var string = "no go here"
	else
		var string = "actually go here"

### Functions
Pluto functions dont require paramaters to be specified if there are none. However, we need to specify argument parenthesis to indicate a function call

	func recursion
	{
		recursion()
	}
	var x = recursion()

### Classes
Pluto classes can extend others, and exhibit much of the standard OO behaviour. Constructors are indicated by a function with the same name as the class

	class Animal
	{
		func Animal(name)
		{
			this.name = name
		}
		func speak
		{
			io::print("my name is "+this.name)
		}
	}
	class Dog extends Animal
	{
		func Dog
		{
			super.Animal("dog")
			super.speak()
		}
	}
	
	var rover = Dog()

### Unusual variable assignment
we can assign any declared value to a variable, e.g. assigning a class to a variable

	class Example
	{
		func Example
		{
			io::print("example")
		}
	}
	var x = Example				  # Assign the example class definition to x
	var y = x()							# Here we create an instance of Example


# Todo

#### Language Design

- Static methods/members
- Class default methods: default 'tostring', 'plus', 'minus' etc
- Abstract classes
- Add more flexible exceptions
     
#### Bug Fix

- Heavily optimise 'visit_binary'
- Increase stack overflow limit
