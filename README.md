![](https://i.imgur.com/jccT97C.png)

***
# What is Pluto3D?
Pluto3D is an interpreted language for use inside of Unity3D. Have you ever wanted your own programming language inside of your computer game? Pluto3D was built just for that!

***

# Simplicity
Pluto was developed to be simple to use, all you have to do to run your code is create a new instance of a pluto runtime and give it your code as a string!

	    PlutoRuntime runtime = new PlutoRuntime();
            runtime.compile(program);

***

# Customization
Pluto supports native code extensions, this allows you to make Pluto do anything you want inside of Unity. Say you want Pluto to create a game object when the user calls a custom function, Pluto can do it!

***

# Safety
Using Pluto is the safest way of allowing access to a scripting langauge in game, typically runtime C# methods always expose the underlying structure of the engine, causing a serious vunerability issue. Pluto only exposes the structure that you want to expose.

***

# Usage
This outlines the basics of using Pluto3D inside of Unity

### PlutoRuntime
First create a PlutoRuntime in a script in your project

	PlutoRuntime runtime = new PlutoRuntime();

### Write your code!
Then write your code! This could be on a canvas, inside C#, from a file... anywhere! Example...

	import "unity.Debug"
	debug::print("Hello Unity Console!")
	# This code prints "Hello Unity Console" to the Console... duh

### Compile
Simply compile the code!

	runtime.compile( your code here! );

### Native extensions
coming soon...
