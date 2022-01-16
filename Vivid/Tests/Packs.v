pack Foo {
	x: large
	y: small
}

pack Bar {
	a: Foo
	x: Foo
	b: Foo
}

# Test: Create a pack and return it
pack_1(x: large, y: large) {
	foo: Foo
	foo.x = y
	foo.y = x
	=> foo
}

# Test: Receive a pack as a parameter and use it
pack_2(foo: Foo) {
	=> foo.x * foo.x + foo.y * foo.y
}

# Test: Receive so many packs that stack must be used
pack_3(a: Foo, b: Foo, c: Foo, d: Foo) {
	=> a.x * b.x * c.x * d.x + a.y * b.y * c.y * d.y
}

# Test: Create a nested pack and return it
pack_4(x: large, y: large) {
	a: Foo
	a.x = y
	a.y = x
	b: Foo
	b.x = y * y
	b.y = x * x
	result: Bar
	result.a = a
	result.b = b
	=> result
}

# Test: Create a nested pack (duplicate inside) and return it
pack_5(x: large, y: large) {
	c: Foo
	c.x = y
	c.y = x
	result: Bar
	result.a = c
	result.x = c
	result.b = c
	=> result
}

goo(a: large, x: Bar) {
	=> x.a.x * x.b.x + x.a.y * x.b.y
}

# Test: Receive nested packs
pack_6(x: Bar, y: Bar) {
	=> x.a.x * x.b.x + x.a.y * x.b.y + y.a.x * y.b.x + y.a.y * y.b.y
}

# Test: Loading pack from memory
pack_7(memory: link<Foo>, i: large) {
	foo = memory[i]
	=> foo.x * foo.x + foo.y * foo.y
}

# Test: Store pack in memory
pack_8(memory: link<Foo>, i: large, x: large, y: large) {
	foo: Foo
	foo.x = y
	foo.y = x
	memory[i] = foo
}

# Test: Loading nested pack from memory
pack_9(memory: link<Bar>, i: large) {
	bar = memory[i]
	=> bar.a.x * bar.b.x + bar.a.y * bar.b.y
}

# Test: Store nested pack in memory
pack_10(memory: link<Bar>, i: large, x: large, y: large) {
	bar: Bar
	bar.a.x = y
	bar.a.y = x
	bar.x.x = 0
	bar.x.y = 0
	bar.b.x = y * y
	bar.b.y = x * x
	memory[i] = bar
}

init() {
	a = pack_1(1, 2)
	b = pack_1(3, 5)
	c = pack_1(7, 11)
	d = pack_1(13, 17)
	println(pack_2(c))
	println(pack_3(a, b, c, d))
	
	x = pack_4(19, 23)
	y = pack_5(27, 31)
	println(pack_6(x, y))

	memory = allocate(sizeof(Bar) * 2) as link<large>
	zero(memory, sizeof(Bar) * 2)
	memory[0] = 37
	memory[1] = 41

	println(pack_7(memory, 0))

	pack_8(memory, 2, 43, 47)
	println(pack_7(memory, 2))

	println(pack_9(memory, 0))

	pack_10(memory, 0, 53, 59)
	println(pack_9(memory, 0))
	=> 0
}