export special_multiplications(a: large, b: large) {
	return 2 * a + b * 17 + a * 9 + b / 4
}

init() { 
	special_multiplications(1, 1)
	return 1
}