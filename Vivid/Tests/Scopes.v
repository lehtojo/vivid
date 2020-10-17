import large_function()

export scopes_nested_if_statements(a, b, c, d, e, f, g, h) {
	x = 2 * a
	y = 3 * b
	z = 5 * c

	if a > 0 {
		if c > 0 {
			large_function()
		}

		large_function()
	}
	else b > 0 {
		if d > 0 {
			large_function()
		}

		large_function()
	}
	else {
		if e > 0 {
			large_function()
		}

		large_function()
	}

	=> (a + b + c + d + e + f + g + h) * x * y * z
}

export scopes_single_loop(a, b, c, d, e, f, g, h) {
	x = 2 * a
	y = 3 * b
	z = 5 * c

	loop (i = 0, i < h, ++i) {
		large_function()
	}

	=> (a + b + c + d + e + f + g + h) * x * y * z
}

export scopes_nested_loops(a, b, c, d, e, f, g, h) {
	x = 2 * a
	y = 3 * b
	z = 5 * c

	loop (i = 0, i < h, ++i) {
		loop (j = 0, j < g, ++j) {
			large_function()
		}

		large_function()
	}

	=> (a + b + c + d + e + f + g + h) * x * y * z
}

init() {
	=> 1
	scopes_nested_if_statements(0, 0, 0, 0, 0, 0, 0, 0)
	scopes_single_loop(0, 0, 0, 0, 0, 0, 0, 0)
	scopes_nested_loops(0, 0, 0, 0, 0, 0, 0, 0)
}