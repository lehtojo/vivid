import internal_print(text: link, length: large)
import internal_read(buffer: link, length: large): large

CONSOLE_READ_LINE_BUFFER_SIZE = 256

###
Summary: Reads text from the console until a line ending is encountered.
NOTE: The maximum length of the returned text is determined by the global constant CONSOLE_READ_LINE_BUFFER_SIZE.
###
export readln() {
	buffer = allocate(CONSOLE_READ_LINE_BUFFER_SIZE)
	length = internal_read(buffer, CONSOLE_READ_LINE_BUFFER_SIZE) - 2

	if length <= 0 {
		deallocate(buffer)
		=> String('')
	}

	result = allocate(length + 1)
	result[length] = 0

	copy(buffer, length, result)
	
	deallocate(buffer)

	=> String(result)
}

###
Summary: 'Reads text from the console until a line ending is encountered.
The maximum length of the returned text is determined by the specified length.'
###
export readln(length: large) {
	buffer = allocate(length)
	length = internal_read(buffer, length) - 2

	if length <= 0 {
		deallocate(buffer)
		=> String('')
	}

	result = allocate(length + 1)
	result[length] = 0

	copy(buffer, length, result)
	
	deallocate(buffer)

	=> String(result)
}

###
Summary: Writes the specified string to the console
###
export print(text: String) {
	internal_print(text.data(), text.length)
}

###
Summary: Writes the specified text to the console
###
export print(text: link) {
	internal_print(text, length_of(text))
}

###
Summary: Converts the specified integer to string and prints it
###
export print(text: large) {
	print(to_string(text))
}

###
Summary: Converts the specified bool to a string and prints it
###
export print(value: bool) {
	if value print('true')
	else { print('false') }
}

###
Summary: Writes the specified string and a line ending to the console
###
export println(text: String) {
	internal_print(text.append(10).data(), text.length + 1)
}

###
Summary: Writes the specified text and a line ending to the console
###
export println(text: link) {
	print(String(text).append(10))
}

###
Summary: Converts the specified number to a string and writes it and a line ending to the console
###
export println(number: tiny) {
	println(to_string(number as large))
}

###
Summary: Converts the specified number to a string and writes it and a line ending to the console
###
export println(number: small) {
	println(to_string(number as large))
}

###
Summary: Converts the specified number to a string and writes it and a line ending to the console
###
export println(number: normal) {
	println(to_string(number as large))
}

###
Summary: Converts the specified number to a string and writes it and a line ending to the console
###
export println(number: large) {
	println(to_string(number))
}

###
Summary: Converts the specified number to a string and writes it and a line ending to the console
###
export println(number: decimal) {
	println(to_string(number))
}

###
Summary: Converts the specified bool to a string and writes it and a line ending to the console
###
export println(value: bool) {
	if value println('true')
	else { println('false') }
}

###
Summary: Moves to the next line
###
export println() {
	put(`\n`)
}

###
Summary: Writes the specified character to the console
###
export put(character: tiny) {
	buffer = allocate_stack(48) + 32
	buffer[0] = character
	internal_print(buffer, 1)
	deallocate_stack(48)
}