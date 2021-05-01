export constant_permanence_and_array_copy(source: link, destination: link) {
   # Offset should stay as constant in the assembly code
	offset = 3
	i = 0

	loop (i = 0, i < 10, ++i) {
		destination[offset + i] = source[offset + i]
	}
}

init() {
	=> 1
}