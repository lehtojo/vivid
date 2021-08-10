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

	if a == b return
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
	print(a)
	print(' == ')
	println(b)

	if a == b return
	exit(1)
}