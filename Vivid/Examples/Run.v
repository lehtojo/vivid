List<T> {
	elements: link<T>
	size: large

	init(count: large) {
		elements = allocate(count * sizeof(T)) as link<T>
		size = count
	}

	set(index: large, value: T) {
		elements[index] = value
	}

	get(index: large) {
		=> elements[index]
	}
}

Goo {
	x: large
	y: large

	sum() {
		list = List<decimal>(16)
		list.set(0, x)
		list.set(1, y)
		=> list.get(0) + list.get(1)
	}

	has_value() {
		=> 1 as bool
	}

	get_value() {
		=> x
	}

	virtual magic() {
		=> x * y + x * y
	}
}

multiplication<Ta, Tb>(a: Ta, b: Tb) {
	=> a * b
}

yeet(a: large, b: large, c: large) {
	d = a / c
	yeet(d, d, d)
	=> b
}

get_start(memory: link) {
	r = 0

	loop (i = 0, i < 10, i++) {
		yeet(0, 0, 0)
		r += memory[i]
	}

	=> multiplication<decimal, tiny>(r, r)
}

foo(a: large, goo: Goo, b: large, c: large, d: large, r: large) {
	loop (i = get_start(nameof(Goo)), i < 10, i++) {
		r += i
	}

	=> r + goo.sum() + goo.magic()
}

goo(a: large, b: large) {
	if a > 1 {
		=> a * 2
	}
	else b > a {
		=> a - b
	}

	=> a + b
}

baz(a: large, b: large) {
	c = a / b
	d = b / 8
	=> c + d
}

math(a: decimal, b: decimal) {
	c = a + b
	d = a - b
	e = a * b
	f = a / b
	g = a as large
	=> c + d + e + f + g
}

init() {
	a = 1
	b = 2
	c = a + b
	d = a + b
	e = c - d

	math(a, e)

	goo(10, 11)
	foo(0, Goo(), 0, 0, 0, 0)
	
	baz(0, 0)

	deallocate(0 as link)
	
	=> d
}

export inheritance_case_1(goo: Goo) {
	=> goo is Goo
}

export inheritance_case_2(goo: Goo) {
	if goo is Goo foo {
		foo.x++
	}
}

export lambda_1() {
	=> (a: large, b: large) -> a + b
}

export lambda_2(x: tiny, y: small, z: normal, w: large, i: decimal) {
	=> () -> x + y + z + w + i
}

export execute_lambda_1(function: () -> decimal) {
	=> function()
}

export has_1(goo: Goo) {
	if goo has value => value
	=> -1
}

export use_extension(goo: Goo) {
	goo.increment()
}

Goo.increment() {
	x++
	y++
}

export numerical_when(x: large) {
	=> when(x) {
		7 => x * x
		3 => x + x + x
		1 => -1
		else => x
	}
}

allocate(bytes: large) {
	a = 4
	b = 2
	c = a * b
	i = 41 * a
	=> a + b + c + i
}

deallocate(address: link) {
	if compiles { address.member } { address++ }
	=> compiles { address + 1 }
}

internal_is(a: link, b: link) {
	=> 0
}