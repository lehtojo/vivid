# Vivid
Small self-hosted and dependency-free compiler for a programming language focusing on comfortability with a nice mix of powerful features.

**Note: This is the project for the first compiler version and there's a [self-hosted](https://github.com/lehtojo/vivid-2) version with more capabilities** 

## Installation

### Precompiled binaries
1. Go to the [releases tab](https://github.com/lehtojo/vivid-2/releases) and download the latest release
2. Open terminal and go to the folder containing the downloaded compiler.
3. If you're on Linux, make the compiler executable by running `chmod +x ./Vivid`
4. You can test compiler by executing ```./Vivid -help```


### Building from source
1. Build the first compiler built with C#

Install [.NET 8](https://dotnet.microsoft.com/en-us/download) or later for building the first compiler

**Note**: You might need to include `dotnet` in your PATH environment variable and restart your terminal
```ps
# Windows (Powershell):
git clone https://github.com/lehtojo/vivid
cd vivid/Vivid/
./Build.bat
```

```bash
# Linux:
git clone https://github.com/lehtojo/vivid
cd vivid/Vivid/
chmod +x ./Build.sh
./Build.sh
```

2. Done. After successful compilation `bin/Release/net8.0/` should contain `Vivid.exe` on Windows or `Vivid` on Linux.

## Targets
Operating systems:
- Windows
- Linux

Processor architectures:
- x86-64

There was a small effort for ARM64. However, that support hasn't been maintained.

## Usage

Running `Vivid -help` will list all available command line options.

```bash
# Building a source file without any libraries
Vivid source.v

# Building a source folder without any libraries
Vivid source/

# Specifying output name
Vivid source/ -o app

# Including the core library (standard library).
# Note: Expects 'core.lib' or 'core.a' to be present
Vivid source/ -l core

# Including the core library by source (Windows).
# Note: Expects the core library source folder 'libv' to be present.
Vivid source/ libv/ libv/windows-x64/ libv/allocator/allocator.v min.math.obj min.memory.obj min.tests.obj -l kernel32.dll

# Including the core library by source (Linux)
Vivid source/ libv/ libv/linux-x64/ libv/allocator/allocator.v min.math.o min.memory.o min.system.o min.tests.o

# Enabling debug information
Vivid source/ -debug

# Outputting generated assembly
Vivid source/ -a
```

## Editors

See repository for [Visual Studio Code extension](https://github.com/lehtojo/vivid-extension).

Primitive Vim syntax file (place in Vim syntax folder):
```vim
syntax match Keywords /\v(<action>|<and>|<or>|<constant>|<continue>|<compiles>|<deinit>|<else>|<export>|<global>|<false>|<finally>|<has>|<if>|<import>|<in>|<init>|<inline>|<is>|<loop>|<nameof>|<mutating>|<namespace>|<none>|<not>|<open>|<outline>|<override>|<pack>|<plain>|<private>|<protected>|<public>|<readable>|<return>|<shared>|<sizeof>|<strideof>|<stop>|<super>|<this>|<true>|<when>)/

syntax match VariableDeclaration /\zs\w\+\ze *\: */

syntax match VariableDeclarationType1 /\zs\: *[a-zA-Z0-9_\.]\+\ze\v($|[^<])/
syntax match VariableDeclarationType2 /\zs\<as\> *[a-zA-Z0-9_\.]\+\ze\v($|[^<])/

syntax match TemplateArguments1 /\zs[a-zA-Z0-9_\.]\+\ze[<>]/
syntax match TemplateArguments2 /<\zs[a-zA-Z0-9_\.]\+\ze\v($|[^<])/

syntax match Comment /#.*/

syntax match SinglyQuotedString1 /\zs\'[^\']*\\\ze\'/
syntax match SinglyQuotedString2 /\'[^\']*\'/

syntax match DoublyQuotedString1 /\zs\"[^\"]*\\\ze\"/
syntax match DoublyQuotedString2 /\"[^\"]*\"/

syntax match Character1 /\zs\`[^\`]*\\\ze\`/
syntax match Character2 /\`[^\`]*\`/

syntax match MemberAccess1 /\zs\w\+\ze\./
syntax match MemberAccess2 /\.\zs\w\+\ze/

syntax match XFunction /\zs\w\+ *\ze(/

syntax match UsingExpression /) *using /

syntax match DotCast /\.\zs([a-zA-Z0-9_\.]\+)\ze/
```

You'll also have to give colors to the syntax patterns. Here's an example of how to do it. Place the following inside Vim's `init.lua` :
```lua
vim.cmd("highlight Keywords ctermfg=red guifg=#d43552")
vim.cmd("highlight VariableDeclaration ctermfg=cyan guifg=#ae9513")
vim.cmd("highlight VariableDeclarationType1 ctermfg=red guifg=#6684e1")
vim.cmd("highlight VariableDeclarationType2 ctermfg=red guifg=#6684e1")
vim.cmd("highlight TemplateArguments1 ctermfg=red guifg=#6684e1")
vim.cmd("highlight TemplateArguments2 ctermfg=red guifg=#6684e1")
vim.cmd("highlight Comment ctermfg=green guifg=#1fad83")
vim.cmd("highlight SinglyQuotedString1 ctermfg=yellow guifg=#60ac39")
vim.cmd("highlight SinglyQuotedString2 ctermfg=yellow guifg=#60ac39")
vim.cmd("highlight DoublyQuotedString1 ctermfg=yellow guifg=#60ac39")
vim.cmd("highlight DoublyQuotedString2 ctermfg=yellow guifg=#60ac39")
vim.cmd("highlight Character1 ctermfg=yellow guifg=#60ac39")
vim.cmd("highlight Character2 ctermfg=yellow guifg=#60ac39")
vim.cmd("highlight MemberAccess1 ctermfg=cyan guifg=#b65611")
vim.cmd("highlight MemberAccess2 ctermfg=cyan guifg=#b65611")
vim.cmd("highlight XFunction ctermfg=yellow guifg=#ae9513")
vim.cmd("highlight UsingExpression ctermfg=red guifg=#d43552")
vim.cmd("highlight DotCast ctermfg=red guifg=#6684e1")
vim.cmd("autocmd BufNewFile,BufRead *.v set filetype=v")
```

## Projects

Projects that use this programming language:
- This project
- [Small kernel](https://github.com/lehtojo/kernel)

## Programming language
If you want to see something practical, see [project section](#projects) above.

### Contents
- [Variables](#variables)
- [Keywords](#keywords)
- [Operators](#operators)
- [Manual memory access](#manual-memory-access)
- [Casting](#casting)
- [Control flow](#control-flow)
    - [Conditional statements](#conditional-statements)
    - [When-statements](#when-statements)
    - [Forever-loops](#forever-loops)
    - [Conditional loops](#conditional-loops)
    - [Command keywords](#command-keywords)
    - [For-loops](#for-loops)
    - [Iteration loops](#iteration-loops)
- [Functions](#functions)
    - [Normal functions](#normal-functions)
    - [Template functions](#template-functions)
    - [Entry point](#entry-point)
    - [Mandatory functions](#mandatory-functions)
- [Types](#types)
    - [Normal type definition](#normal-type-definition)
    - [Pack type definition](#pack-type-definition)
    - [Constructors](#constructors)
    - [Access modifiers](#access-modifiers)
    - [Member functions](#member-functions)
    - [Template types](#template-types)
    - [Operator overloading](#operator-overloading)
    - [Inheritance](#inheritance)
    - [Open functions](#open-functions)
    - [Expression variables](#expression-variables)
    - [Namespaces and imports](#namespaces-and-imports)
    - [Non-heap based allocation](#non-heap-based-allocation)
- [String objects](#string-objects)
- [Compiles-expressions](#compiles-expressions)
- [Is-expressions](#is-expressions)
- [Has-expressions](#has-expressions)
- [Ranges](#ranges)
- [Iteration loops](#iteration-loops-1)
- [Lambdas](#lambdas)
- [Using-expressions](#using-expressions)
- [Deinitializer-statements](#deinitializer-statements)
- [Macros](#macros)

### Variables
```python
# Integers variable (64-bit by default):
i1 = 1
# Specifying type explicitly:
i2: u32 = 2
i3 = 2u16
# Floating point number variable (64-bit):
f1: decimal = 3.14159
f2: decimal = -42e7
# Boolean variable:
b1: bool = true
# Character variables:
c1 = `v`
c2 = `\n`
c3 = `\x2a`
# C-style string pointers:
s1: u8* = 'Hello there :^)'
```

See [string objects](#string-objects) below.

### Keywords
Reserved keywords can't be used as identifiers in user's code:
|            |            |            |            |
|------------|------------|------------|------------|
| as         | has        | outline    | return     |
| compiles   | if         | override   | shared     |
| constant   | in         | pack       | stop       |
| continue   | import     | plain      | using      |
| deinit     | loop       | private    | when       |
| else       | namespace  | protected  |            |
| export     | not        | public     |            |
| global     | open       | readable   |            |

Non-reserved keywords are `init` and `this` for instance.

### Operators

```python
a = 7
b = 42
c = true
d = false

# Arithmetic operators:
addition = a + b
subtraction = a - b
multiplication = a * b
division = a / b
remainder = a % b

# Bitwise operators:
bitwise_and = a & b
bitwise_or = a | b
bitwise_xor = a ¤ b
bitwise_not = !a
bitwise_shift_left = a <| b
bitwise_shift_right = a |> b

# Logical operators:
logical_and = c and d
logical_or = c or d
logical_not_1 = not c
logical_not_2 = !d

# Comparison operators:
equal = a == b
not_equal = a != b
absolute_equal = a === b
not_absolute_equal = a === b
greater_than = a > b
less_than = a < b
greater_than_or_equal = a >= b
less_than_or_equal = a <= b

# Assigning operators:
a = b
a += b
a -= b
a *= b
a /= b
a %= b
a &= b
a |= b
a ¤= b

# Increment operators
a = ++b
a = b++
a = --b
a = b--

# Unary sign operator
a = -b
```

### Manual memory access
```python
pointer: u32* = 0x12345678

first_element = pointer[0]
first_element = pointer[]

third_element = pointer[1 + 1]
```

### Casting
Conversion between types can be achieved by using as-expressions.
```python
i1 = 3
f1 = 3.14159

i2 = f1 as i32     # i2 = 3
f2 = i1 as decimal # f2 = 3.0

m1 = 0x12345678 as u8*
```
There's also 'dot casting' that is introduced [below](#is-expression).

### Control flow

#### Conditional statements
```python
a = 7
b = 42
largest = 0

if a > b {
    # Executed if a > b
    largest = a
} else a < b {
    # Executed if a < b
    largest = b
} else {
    # Executed if a == b
    largest = a
}
```

#### When-statements
```python
x = 42

result = when(x) {
    < 10 => 'X is less than 10',
    10 => 'X is 10',
    else => 'X is greater than 10'
}
# Equilevant:
# result = 0
# 
# if x < 10 {
#     result = 'X is less than 10'
# } else x == 10 {
#     result = 'X is 10'
# } else {
#     result = 'X is greater than 10'
# }
```

#### Forever-loops
```python
loop {
    # Executed for ever
}
```

#### Conditional loops
```python
i = 0

# Stops once i is equal to 10
loop (i < 10) {
    i++
}
```

#### Command keywords
```python
i = 0

loop {
    # Stops once i is equal to 10
    if i == 10 {
        stop
    }

    i++
}

i = 0

loop {
    if i < 10 {
        i++
        continue
    }

    # Stops once i is equal to 10
    stop
}
```

#### For-loops
```python
loop (i = 0, i < 10, i++) {
    # Repeats 10 times
}

# You can also extract the variable definition
j = 0

loop (j < 10, j++) {
    # Repeats 10 times
}

# Here j is equal to 10
k = j
```

#### Iteration loops

See [below](#iteration-loops-1).

### Functions
#### Normal functions
```python
# Here we define a function that adds the parameters together and returns the result.
# We don't specify the parameter types, so they can be anything. This applies to the return type as well.
addition(a, b) {
    return a + b
}

# Here we specify that 'b' must be an integer, but 'a' can be anything. Therefore the return type can still be anything.
subtraction(a, b: i32) {
    return a - b
}

# Here we fully specify the function signature.
multiplication(a: u16, b: u16, c: u16): u64 {
    return a * b * c
}

# Here's an empty function.
empty() {}

# Here's how you explicitly specify function returning nothing.
nothing(): _ {}

# Here's how you call the functions.
test() {
    r1 = addition(1, 3)
    r2 = subtraction(7, 10)
    r3 = multiplication(r1, r2, 42)

    # You can also assign and return nothing
    r4 = empty()
    return nothing()
}
```

#### Template functions
```python
# If you need to pass type information into the function, you can use template functions.
addition<T1, T2>(a: T1, b: T1): T2 {
    result: T2 = a + b
    return result
}

# Template functions are created during compilation time.
subtraction(a: i64, b: i64): i32 {
    # Here's how you use the template function.
    return addition<i64, i32>(a, b)
}

# During compilation the following is generated:
addition(a: i64, b: i64): i32 {
    result: i32 = a + b
    return result
}
```

#### Entry point
When the application starts, the first function to be called is `internal_init`, which calls `init` afterwards.
```
internal_init(stack_pointer: u8*) {
    # This is the first function that is called when the application starts.
    # This function is intended for hidden library functionality.

    # After internal initialization, the user entry function is called
    init()
}

init() {
    # Place your code here
}
```

#### Mandatory functions

All applications must implement the following functions in order to be compiled:

| Signature                                                     | Description |
|---------------------------------------------------------------|-------------|
| init(): [ i64 \| _ ]                                          | User entry point function that optionally returns the application's exit code. |
| internal_init(stack_pointer: u8*): [ i64 \| _ ]               | Entry point function that optionally returns the application's exit code. The function receives the stack pointer as a parameter that can be used to extract command line arguments passed by the kernel on Linux. This function is auto-generated if it's not implemented. |
| internal_is(virtual_table_1: u8*, virtual_table_2: u8*): bool | Compares the two virtual tables and returns whether they are equal or one inherits the another. Used to implement the is-expressions. |
| allocate(size: u64): u8*                                      | Global heap allocation function. |
| deallocate(address: u8*): _                                   | Global heap deallocation function. |

### Types

#### Normal type definition
Normal types are heap allocated and passed by reference.
```python
# Here's a simple String object type
String {
    # Here we have two public member variables
    data: u8*
    size: u64
}

string(data: u8*, size: u64): String {
    string = String()

    # Here we assign the parameter values to the member variables
    string.data = data
    string.size = size

    return string
}
```
The code above is equilevant to the following C++ code:
```cpp
class String {
public:
    unsigned char* data;
    unsigned long long size;
};

String* string(unsigned char* data, unsigned long long size) {
    // Here the 'allocate' function could be a heap allocation function defined by the user or the standard library
    String* string = (String*)allocate(24);

    string->data = data;
    string->size = size;

    return string;
}
```

#### Pack type definition
Pack types are types passed by value. They're equilevant to structs in C-languages.
```python
pack String {
    private data: u8*
    readable size: u64

    shared new(data: u8*, size: u64): String {
        return pack {
            data: data,
            size: size
        } as String
    }
}

init() {
    string = String.new('Hello there :^)', 15)
    # ...
}
```

#### Constructors

Instead of creating the string helper function, the user could define a constructor function:
```python
String {
    data: u8*
    size: u64

    init(data: u8*, size: u64) {
        this.data = data
        this.size = size
    }
}

# Application entry point:
init() {
    # Here's how to call the constructor with arguments
    string = String('Hello there :^)', 15)
}
```

#### Access modifiers

The user could also restrict the access to the member variables as follows:
```python
String {
    private data: u8*
    readable size: u64

    private readable useless: i32

    init(data: u8*, size: u64) {
        # Private member variables can be accessed in the type's member functions
        this.data = data
        # Readable modifier only removes write access from public access level, therefore the following is allowed
        this.size = size

        # The line below generates a compilation error as we're requesting write access from private access level and that write access has been removed with the readable modifer
        this.useless = 42

        # However, we still have read access to it
        this.size = this.useless
    }
}

# Application entry point:
init() {
    # Here's how to call the constructor with arguments
    string = String('Hello there :^)', 15)

    # We have read access to the size member variable
    size = string.size

    # The line below generates a compilation error as we're requesting a read access to a private member variable from public access level
    data = string.data
    # The line below generates a compilation error as we're requesting write access from public access level and that write access has been removed with the readable modifer
    string.size = 0
}
```
There's also support for `protected` access modifier that is useful for inheritance.

#### Member functions
```python
String {
    # Shared access modifier is equilevant to 'static' keyword in many languages.

    # Shared member variables can be accessed without and instance of the type.
    shared empty: String

    # Shared member functions can be called without an instance of the type.
    shared initialize() {
        empty = String('', 0)
    }

    private data: u8*
    readable size: u64

    init(data: u8*, size: u64) {
        this.data = data
        this.size = size
    }

    index_of(character: u8): i64 {
        loop (i = 0, i < size, i++) {
            if data[i] === character {
                return i
            }
        }

        return -1
    }
}

find_first_line_ending(string: String) {
    return string.index_of('\n')
}

# Application entry point:
init() {
    String.initialize()
}
```

#### Template types
```python
Array<T> {
    private data: T*
    readable size: u64

    init(size: u64) {
        this.size = size
    }
}

# Application entry point:
init() {
    # Creates an array with size of 10
    array = Array<i32>(10)
}
```

#### Operator overloading
```python
Array<T> {
    private data: T*
    readable size: u64

    init(size: u64) {
        this.size = size
    }

    set(i: u64, value: T) {
        # Assign the value to the data at i:th index
        data[i] = value
    }

    get(i: u64) {
        return data[i]
    }
}

init() {
    array = Array<i32>(10)

    loop (i = 0, i < array.size, i++) {
        array[i] = i * i
        # Equilevant:
        # array.set(i, i * i)
    }

    return array[0] + array[array.size - 1]
    # Equilevant:
    # return array.get(0) + array.get(array.size - 1)
}
```
Other operators:

| Operator | Function         |
| -------- | ---------------- |
| +        | plus             |
| -        | minus            |
| *        | times            |
| /        | divide           |
| %        | remainder        |
| +=       | assign_plus      |
| -=       | assign_minus     |
| *=       | assign_times     |
| /=       | assign_divide    |
| %=       | assign_remainder |
| ==       | equals           |

#### Inheritance
```python
Animal {
    name: String

    init(name: String) {
        this.name = name
    }
}

# Here we define 'Cat' type that inherits all 'Animal' type's properties
Animal Cat {
    meow() {
        console.write_line('Meow!')
    }
}

# Here we define 'Dog' type that inherits all 'Animal' type's properties
Animal Dog {
    bark() {
        console.write_line('Woof!')
    }
}
```

#### Open functions
Open functions are equilevant to virtual functions in many languages. Open functions are internally implemented using virtual tables.
```python
Animal {
    name: String

    init(name: String) {
        this.name = name
    }

    # Open function implemented or overriden by inheriting types
    open react() {
        # No reaction by default
    }

    open color(): u32
}

# Virtual table can be removed by using the 'plain' modifier
plain String {
    data: u8*
    size: u64

    # ...
}

# Here we define Cat type that inherits all Animal type's properties
Animal Cat {
    meow() {
        console.write_line('Meow!')
    }

    # Here we override the functionality of the base 'react' member function.
    override react() {
        meow()
    }

    # Here we implement the base 'color' member function.
    override color() {
        return 0xff5533
    }
}

# Here we define Dog type that inherits all Animal type's properties
Animal Dog {
    bark() {
        console.write_line('Woof!')
    }

    # Here we override the functionality of the base 'react' member function.
    override react() {
        bark()
    }

    # Here we implement the base 'color' member function.
    override color() {
        return 0x424242
    }
}

init() {
    animal1: Animal = Cat()
    animal2: Animal = Dog()

    animal1.react() # Prints 'Meow!'
    animal2.react() # Prints 'Woof!'
}
```

#### Expression variables
```python
constant UNSIGNED_FLAG = 1

plain Format {
    private data: u8

    is_unsigned => (data & UNSIGNED_FLAG) != 0
    bits => data |> 1
    # Equilevant:
    # is_unsigned() { return (data & UNSIGNED_FLAG) != 0 }
    # bits() { return data |> 1 }

    private init(bits: u8, is_unsigned: bool) {
        this.data = (bits <| 1) | is_unsigned
    }

    shared unsigned(bits: u8) {
        return Format(bits, true)
    }

    shared signed(bits: u8) {
        return Format(bits, false)
    }
}

init() {
    format1 = Format.unsigned(32)
    format2 = Format.signed(64)

    console.write_line(format1.is_unsigned) # Prints 'true'
    console.write_line(format2.bits) # Prints '64'
}
```

#### Namespaces and imports
```python
# Namespaces are equilevant to types with 'shared' modifier
namespace system.console {
    # Equilevant:
    # shared system {
    #     shared console {
    #         # ...
    #     }
    # }

    # Import an external function for writing into console
    import 'C' write_line_implementation(string: u8*): _

    write_line(string: link) {
        write_line_implementation(string)
    }
}

# Properties of namespace can be imported as follows
import system

init() {
    console.write_line('Hello there :^)')
}
```

#### Non-heap based allocation
See [using-expressions](#using-expressions) and [pack types](#pack-type-definition).

### String objects
```python
string = "This is a string object"
# Equilevant:
# string = String('This is a string object')
```

### Type inspection
```python
plain String {
    private data: u8*
    readable size: u64
}

pack Pair {
    first: i32
    second: i32
}

init() {
    console.write_line(sizeof(u16))      # Prints '2'
    console.write_line(sizeof(u32))      # Prints '4'
    console.write_line(sizeof(u8*))      # Prints '8'
    console.write_line(sizeof(String))   # Prints '16'
    console.write_line(sizeof(Pair))     # Prints '16'

    console.write_line(strideof(u16))    # Prints '2'
    console.write_line(strideof(u32))    # Prints '4'
    console.write_line(strideof(u8*))    # Prints '8'
    console.write_line(strideof(String)) # Prints '8'
    console.write_line(strideof(Pair))   # Prints '16'

    console.write_line(nameof(u16))      # Prints 'u16'
    console.write_line(nameof(u32))      # Prints 'u32'
    console.write_line(nameof(u8*))      # Prints 'u8*'
    console.write_line(nameof(String))   # Prints 'String'
    console.write_line(nameof(Pair))     # Prints 'Pair'
}
```

### Compiles-expressions
```python
Map<K, V> {
    # ...

    add(key: K, value: V) {
        hash = key as i64

        # If the key has a 'hash' member function,
        # the compiles-expression will simplify to 'true' and the if-statement will be unwrapped.
        # Otherwise the whole if-statement is removed.
        # Compiles-expression will evaluate to 'true' when the expression inside doesn't emit any errors.
        # Otherwise it'll be evaluated to 'false'.
        if compiles { key.hash() } {
            hash = key.hash()
        }

        # ...
    }

    # ...
}
```

### Is-expressions
Runtime type inspection can be achieved by using is-expressions. By default, is-expressions are implemented using virtual tables. Is-expressions call `internal_is` function that can be implemented by the user or the standard library for example.
```python
# We can't add 'plain' modifier, because we need virtual tables
Animal {
    # ...
}

Animal Cat {
    name: u8* = 'Cat'
    # ...
}

Animal Dog {
    name: u8* = 'Dog'
    # ...
}

init() {
    animal1 = Cat()
    animal2 = Dog()

    if animal1 is Cat cat {
        # Equilevent:
        # if internal_is(<virtual table of animal1>, <virtual table of Cat>) {
        #     cat = animal1 as Cat
        #     # ...
        # }
        console.write_line(cat.name) # Prints 'Cat'
    }

    if animal2 is not Cat {
        # Equilevent:
        # if !internal_is(<virtual table of animal2>, <virtual table of Cat>) {
        #     # ...
        # }

        # Dot casting:
        console.write_line(animal2.(Dog).name) # Prints 'Dog'
        # Equilevant:
        # console.write_line((animal2 as Dog).name)
    }
}
```

### Has-expressions
```python
Optional<T> {
    value: T
    empty: bool

    init() {
        empty = true
    }

    init(value: T) {
        this.value = value
        this.empty = false
    }

    has_value() {
        return not empty
    }

    get_value() {
        return value
    }
}

init() {
    o1 = Optional<i64>(7)
    o2 = Optional<i64>()

    if o1 has value {
        console.write_line(value) # Prints '7'
    }
    # Equilevant:
    # if o1.has_value() {
    #     value = o1.get_value()
    #     # ...
    # }

    if o2 has not value {
        console.write_line('No value') # Prints 'No value'
    }
    # Equilevant:
    # if !o2.has_value() {
    #     value = o2.get_value()
    #     # ...
    # }
}
```

### Ranges
Range-expressions are converted into objects:
```python
init() {
    n = 42
    range1 = 0..10
    range2 = -1..(2 * n)
    # Equilevant:
    # n = 42
    # range1 = Range(0, 10)
    # range2 = Range(-1, (2 * n))
}
```

### Iteration loops
```python
Array<T> {
    private data: T*
    readable size: u64

    init(size: u64) {
        this.size = size
    }

    set(i: u64, value: T) {
        # Assign the value to the data at i:th index
        data[i] = value
    }

    get(i: u64) {
        return data[i]
    }

    iterator(): SequentialIterator<T> {
        return SequentialIterator<T>(data, size)
    }
}

SequentialIterator<T> {
    data: T*
    position: u64
    size: u64

    init(data: T*, size: u64) {
        this.data = data
        this.position = -1
        this.size = size
    }

    value() {
        return data[position]
    }

    next() {
        return ++position < size
    }
}

init() {
    array = Array<i32>(3)
    array[0] = 1
    array[1] = 2
    array[2] = 3

    # Prints the array's elements
    loop element in array {
        console.write_line(element)
    }
    # Equilevant:
    # loop (iterator = array.iterator(), iterator.next(), ) {
    #     element = iterator.value()
    #     # ...
    # }
}
```

### Lambdas
Lambdas are converted into heap allocated objects. Lambdas can capture variables from the visible scope.
```python
List<T> {
    # ...

    filter(filter: (T) -> bool) {
        result = List<T>()

        loop (i = 0, i < size, i++) {
            if filter(data[i]) {
                result.add(data[i])
            }
        }

        return result
    }

    # ...
}

init() {
    lines = List<String>()

    # ...

    # Searches all lines that contain the term string
    term = '...'
    lines = lines.filter((line: String) -> line.contains(term))

    # ...
}
```

### Using-expressions
Using-expressions can be used for specifying the allocator to use for allocating an object.
```python
# ...

PhysicalMemoryManager {
    shared instance: PhysicalMemoryManager

    shared initialize(memory_information: kernel.SystemMemoryInformation) {
        # Allocates the object at memory address specified by 'memory_information.physical_memory_manager_virtual_address'.
        instance = PhysicalMemoryManager(memory_information) using memory_information.physical_memory_manager_virtual_address
    }
}

# ...

create_kernel_thread(rip: u64): Process {
    # ...
    # The allocator can be a type.
    # Allocates the object by calling 'KernelHeap.allocate'.
    memory = ProcessMemory(HeapAllocator.instance) using KernelHeap
    # ...
}

# ...

TimerManager {
    timers: List<Timer>
    registers: u64*

    init(allocator: Allocator) {
        # The allocator can be a variable.
        # Allocates the object by calling 'allocator.allocate'.
        timers = List<Timer>(allocator) using allocator
    }

    # ...
}
```

### Deinitializer-statements
Deinitializer-statements can be useful for cleanup code. Deinitializer-statements are executed when the function exits.
```python
buffer = allocate(PAGE_SIZE)
deinit { deallocate(buffer) }

# ...

if not validate(buffer) {
    # Executed before exiting the function:
    # deinit { deallocate(buffer) }
    return EINVAL
}

# ...

# Executed before exiting the function:
# deinit { deallocate(buffer) }
return 0
```
### Macros
```php
$add_to_list!(list) {}

$add_to_list!(list, x, elements...) {
    $list.add($x)
    add_to_list!($list, $elements...)
}

$list_of!(T, elements...) {
    $list = List<$T>()
    add_to_list!($list, $elements...)
    $list
}

$loop!(n) {
    loop ($i = 0, $i < $n, $i++)
}

$print!() {}

$print!(arguments..., argument) {
    print!($arguments...)
    console.write($argument)
}

$foreach!(i, collection, body) {
    loop ($l = $collection.iterator(), $l.next(), ) {
        $i = $l.value()
        $body
    }
}

# Outputs:
# Hello there :^)!
# Hello there :^)!
# Hello there :^)!
# Hello there again :^)!
# Elements: 
# 3
# 7
# 14
# 42
# Sum: 66
# Goodbye!
init() {
    loop!(3) {
        console.write_line("Hello there :^)!")
    }

    loop!(1) { console.write_line("Hello there again :^)!") }

    list = list_of!(u32, 3, 7, 8 + 6, 42)
    sum = 0

    print!('Elements: \n')

    foreach!(i, list, 
        print!(i, '\n')
        sum += i
    )

    print!('Sum: ', sum, '\n', 'Goodbye!\n')
    return 0
}
```

## Limitations
- Suffers from extensive memory usage
  - However, there's an incomplete implementation for reference counting. It has to be enabled manually.
- No proper incremental or multi-threaded build support
  - However, there's library support
- No advanced optimizations. Supported optimizations:
  - Function inlining
  - Dead code elimination
  - Simple algebraic optimizations
  - Loop unwrapping and extraction
- Heuristical linear scan register allocator that doesn't take live range holes into account
- Optimizing code size isn't supported. Machine code always uses 64-bit instructions if possible.

## License
Vivid is distributed under the terms of MIT License. See [LICENSE](./LICENSE) file.