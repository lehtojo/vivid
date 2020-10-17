g(x) {
   => x + 1
}

f(x) {
   => x * g(x)
}

h(x) {
   => 2 * x
}

init() {
	inlines_members()
	inlines_conditional_statements(1, 2)

	y = 0
	s = f(7)

	loop (i = 0, i < 10, ++i) {
		y += s
	}

	=> y
}

Type {
	a: num
	b: num

	init(a, b) {
		this.a = a
		this.b = b
	}

	get_sum() {
		=> a + b
	}
}

outline inlines_members() {
	type = Type(1, 2)
	
	c = type.get_sum()

	=> type.a + type.b + c - 2 * type.get_sum()
}

outline inlines_conditional_statements(a, b) {
	#a = 1
	#b = -1
	c = 0

	if f(a + b) > g(a * b) {
		=> f(f(a))
	}
	else g(a) > h(b) * f(a) {
		c = f(a) + h(b)
		=> c + 1
	}

	=> a + b
}
















import large_function()

#init() {
 #  y = 1 + 2
 #  large_function()
 #  z = y + 1
 #  large_function()
 #  large_function()
#   w = 1 + 2
#   large_function()

#   loop {
#	  w += 1
#   }
#}
