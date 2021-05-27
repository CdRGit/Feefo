# Feefo
This is a 2d esolang I made for fun, it is Queue based, it is hell

it also has multi threading by way of creating multiple pointers

# Syntax
- `><V^` are used to move the instruction pointer around
- `.>` creates an instruction pointer moving to the right
- `<.` creates an instruction pointer moving to the left
- same works for up and down.

- `!` skips the next instruction always
- `?` skips the next instruction if the front of the queue is zero

- `"FooBar"` puts the ASCII values for FooBar on the queue

- `0 1 2 3 4 5 6 7 8 9` put that value on the queue

- `p` prints the first value as ASCII
- `P` does the same, but with a newline
- `o` outputs the first value as a number
- `O` outputs the first value as a number, with newline
- `h` outputs the first value as hex
- `H` outputs the first value as hex, with newline

- `+` `-` `/` `*` and `%` are supported, they take the first two values and apply themselves ([a, b] `+` [a + b])

- ` ` are ignored

- `i` reads the first char from stdin
- `I` reads an integer from a line in stdin

- `#` adds the amount of items currently in the queue to the end

- `r` recycles the first item in the queue (brings it to the back)
- `R` reverses the entire queue

- `d` duplicates the first item by dequeueing it and then adding it to the end twice
- `D` duplicates by peeking and then enqueueing once

- `,` allows for the construction of meeting points (`,>,`) where both pointers wait and then transfer the value according to the direction of the arrow

- inside a string you can escape values by using \, only `\"`, `\0`, `\n`, `\r` and `\a` are permitted because I could not be bothered to add more

- if a character is not recognized and the pointer is not in string mode, the program should throw