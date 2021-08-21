DECIMAL_PRECISION = 0.000000001

export are_equal(a: large, b: large) {
	print(a)
	print(' == ')
	println(b)

	if a == b return
	exit(1)
}

export are_equal(a: char, b: char) {
	print(String(a))
	print(' == ')
	println(String(b))

	if a == b return
	exit(1)
}

export are_equal(a: decimal, b: decimal) {
	print(to_string(a))
	print(' == ')
	println(b)

	d = a - b

	if d >= -DECIMAL_PRECISION and d <= DECIMAL_PRECISION return
	exit(1)
}

export are_equal(a: String, b: String) {
	print(a)
	print(' == ')
	println(b)

	if a == b return
	exit(1)
}

export are_equal(a: link, b: link) {
	print(a as large)
	print(' == ')
	println(b as large)

	if a == b return
	exit(1)
}

export are_equal(a: link, b: link, offset: large, length: large) {
	print('Memory comparison: Offset=')
	print(offset)
	print(', Length=')
	println(length)

	loop (i = 0, i < length, i++) {
		print(i)
		print(': ')

		x = a[offset + i]
		y = b[offset + i]

		print(to_string(x))
		print(' == ')
		println(to_string(y))

		if x != y exit(1)
	}
}

export are_not_equal(a: large, b: large) {
	print(a)
	print(' != ')
	println(b)

	if a != b return
	exit(1)
}