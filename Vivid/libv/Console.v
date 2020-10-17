import internal_print(text: link, length: num)
import internal_read(buffer: link, length: num) => num

readln() {
	buffer = allocate(256)
	length = internal_read(buffer, 256) - 2

	if length <= 0 {
		free(buffer)
		=> String('')
	}

	result = allocate(length)
	copy(buffer, length, result)
	deallocate(buffer, 256)

	=> String(result)
}

prints(text: String) {
	internal_print(text.data(), text.length())
}

print(text: link) {
	internal_print(text, length_of(text))
}

print_character(character: num) {
	buffer = allocate(1)
	buffer[0] = character
	internal_print(buffer, 1)
	deallocate(buffer, 1)
}

printsln(text: String) {
	internal_print(text.append(10).data(), text.length() + 1)
}

println(text: link) {
	prints(String(text).append(10))
}