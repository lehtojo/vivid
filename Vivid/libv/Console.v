import internal_print(text: link, length: large)
import internal_read(buffer: link, length: large): large

CONSOLE_READ_LINE_BUFFER_SIZE = 256

###
summary: 'Reads text from the console until a line ending is encountered.
The maximum length of the returned text is determined by the global constant CONSOLE_READ_LINE_BUFFER_SIZE.'
###
readln() {
	buffer = allocate(CONSOLE_READ_LINE_BUFFER_SIZE)
	length = internal_read(buffer, CONSOLE_READ_LINE_BUFFER_SIZE) - 2

	if length <= 0 {
		free(buffer)
		=> String('')
	}

	result = allocate(length)
	copy(buffer, length, result)
	deallocate(buffer, CONSOLE_READ_LINE_BUFFER_SIZE)

	=> String(result)
}

###
summary: 'Reads text from the console until a line ending is encountered.
The maximum length of the returned text is determined by the specified length.'
###
readln(length: large) {
	buffer = allocate(length)
	length = internal_read(buffer, length) - 2

	if length <= 0 {
		free(buffer)
		=> String('')
	}

	result = allocate(length)
	copy(buffer, length, result)
	deallocate(buffer, length)

	=> String(result)
}

###
summary: Writes the specified string to the console
###
print(text: String) {
	internal_print(text.data(), text.length())
}

###
summary: Writes the specified text to the console
###
print(text: link) {
	internal_print(text, length_of(text))
}

###
summary: Writes the specified string and a line ending to the console
###
println(text: String) {
	internal_print(text.append(10).data(), text.length() + 1)
}

###
summary: Writes the specified text and a line ending to the console
###
println(text: link) {
	print(String(text).append(10))
}

###
summary: Converts the specified number to a string and writes it and a line ending to the console
###
println(number: tiny) {
	println(to_string(number as large))
}

###
summary: Converts the specified number to a string and writes it and a line ending to the console
###
println(number: small) {
	println(to_string(number as large))
}

###
summary: Converts the specified number to a string and writes it and a line ending to the console
###
println(number: normal) {
	println(to_string(number as large))
}

###
summary: Converts the specified number to a string and writes it and a line ending to the console
###
println(number: large) {
	println(to_string(number))
}

###
summary: Converts the specified number to a string and writes it and a line ending to the console
###
println(number: decimal) {
	println(to_string(number))
}

###
summary: Writes the specified character to the console
###
print_character(character: large) {
	buffer = allocate(1)
	buffer[0] = character
	internal_print(buffer, 1)
	deallocate(buffer, 1)
}