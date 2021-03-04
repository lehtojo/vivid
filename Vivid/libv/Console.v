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
		deallocate(buffer, CONSOLE_READ_LINE_BUFFER_SIZE)
		=> String('')
	}

	result = allocate(length)
	copy(buffer, length, result)
	deallocate(buffer, CONSOLE_READ_LINE_BUFFER_SIZE)

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
		deallocate(buffer, CONSOLE_READ_LINE_BUFFER_SIZE)
		=> String('')
	}

	result = allocate(length)
	copy(buffer, length, result)
	deallocate(buffer, length)

	=> String(result)
}

###
Summary: Writes the specified string to the console
###
export print(text: String) {
	internal_print(text.data(), text.length())
}

###
Summary: Writes the specified text to the console
###
export print(text: link) {
	internal_print(text, length_of(text))
}

###
Summary: Writes the specified string and a line ending to the console
###
export println(text: String) {
	internal_print(text.append(10).data(), text.length() + 1)
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
Summary: Writes the specified character to the console
###
export print_character(character: large) {
	buffer = allocate(1)
	buffer[0] = character
	internal_print(buffer, 1)
	deallocate(buffer, 1)
}