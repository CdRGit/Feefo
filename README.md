# Feefo
This is a 2d esolang I made for fun, it is Queue based, it is hell

# Syntax
- `><V^` are used to move the instruction pointer around
- `.>` creates an instruction pointer moving to the right
- `<.` creates an instruction pointer moving to the left
- etc.

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