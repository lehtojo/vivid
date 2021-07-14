import large_function()

export evacuation(a: large, b: large) {
	c = a * b + 10
	large_function()
	=> a + b + c
}

export evacuation_with_memory(a: large, b: large) {
	c = a * b + 10
	d = a * b + 10
	e = a * b + 10
	large_function()
	=> a + b + c + d + e
}

init() {
	=> 1
}