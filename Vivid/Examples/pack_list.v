# 1. OK
pack Foo {
	a: large
	b: large
}

export pack_list_1() {
	foo: Foo
	foo.a = 1
	foo.b = 2

	foos = List<Foo>()
	foos.add(foo)

	=> 0
}

# 2. OK

export pack_list_2() {
	foo = { a: 1, b: 2 }

	foos = List<{ a: large, b: large }>()
	foos.add(foo)

	=> 0
}

# 3. OK

export pack_list_3() {
	foo = { a: 1, b: 2 }
	bar = { a: -1, b: -2 }
	list = [ foo, bar ]
}

sum_of_packs(elements: List<{ a: large, b: large }>) {
	sum = 0.0

	loop element in elements {
		sum += element.a + element.b
	}

	=> sum
}

init() {
	result = sum_of_packs([{ a: 1, b: 2 }, { a: 3, b: 4 }])
	println(result)
	=> 0
}