export constant_permanence_and_array_copy(source, destination) {
   # Offset should stay as constant in the assembly code
	offset = 3
	i = 0

	loop (i = 0, i < 10, ++i) {
		destination[offset + i] = source[offset + i]
	}
}

init() {
	=> 1

   # Dummy for type resolvation
	constant_permanence_and_array_copy(0 as link, 0 as link)
}