export linkage_1(b) {
	a = b
	b = 1 + a # Since a is linked to b, the compiler could forget to differentiate a from b before this assign 
	=> a + b # If the differentiation fails, the result will be 2b + 2 (On success: 2b + 1)
}

# Parameter b must not be 1
export linkage_2(b) {
	a = b
	a = 1 # Since b is linked to a, the compiler could forget to differentiate b from a before this assign 
	=> 2 * b + 2 * a # If the differentiation fails, the result will be 2 + 2 (On success: 2b + 2)
}

export linkage_3(b) {
	a = b	
	i = 0

	c = b
	d = c
	e = a

	j = i

	loop (i, i < 3, i) {
		a = i
		b = i + 1
		++i
	}

	=> a + b # On success: 5, on failure: 6
}

export linkage_4(b) {
	x = b	
	y = x
	z = y
	w = z

	i = 0

	loop (i, i < 5, ++i) {
		x += 1
		y += 2
		z += 4
		w += 8
	}

	# x = b + 5, y = b + 10, z = b + 20, w = b + 40
	# Success: 4b + 75
	=> x + y + z + w
}

import large_function()

export linkage_5(z) {
	a = z
	b = a
	c = b
	d = c
	e = d
	f = e
	g = f
	h = g
	i = h
	j = i

	large_function()

	l = 0

	loop (l, l < 5, ++l) {
		a += 1
		b += 2
		c += 3
		d += 4
		e += 5
		f += 6
		g += 7
		h += 8
		i += 9
		j += 10
	}

	# a = 5  b = 10 c = 15 d = 20
	# e = 25 f = 30 g = 35 h = 40
	# i = 45 j = 50
	# Success: 275 + 10z
	=> a + b + c + d + e + f + g + h + i + j
}

export linked_variables(x, y) {
	a = x

	loop (i = 0, i < y, ++i) {
		large_function()
	}

	=> a + x
}

export linked_variables_2(x, y) {
	r = 0

	loop (i = 0, i < y, ++i) {
		a = x
		large_function()
		r += a
	}

	=> r + x
}

init() {
	=> 1
	linkage_1(10)
	linkage_2(10)
	linkage_3(10)
	linkage_4(10)
	linkage_5(10)
	linked_variables(0, 0)
	linked_variables_2(0, 0)
}