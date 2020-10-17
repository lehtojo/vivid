import large_function()

export evacuation(a, b) {
	c = a * b + 10
	large_function()
	=> a + b + c
}

export evacuation_with_memory(a, b) {
	c = a * b + 10
	d = a * b + 10
	e = a * b + 10
	large_function()
	=> a + b + c + d + e
}

init() {
	=> 1
	
	# Dummy for type resolvation
	evacuation(1, 1)
	evacuation_with_memory(1, 1)
}