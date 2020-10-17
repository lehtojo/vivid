export simple_math(a, b, c) {
	x = a * c + a + c
	y = b * a * (c + 1) * 100
	=> x + y
}

export basic_if(a, b) {
	if a >= b {
		=> a
	}
	else {
		=> b
	}
}

init() {
	# Dummy for type resolving
	simple_math(1, 2, 3)
	basic_if(1, 2)
}