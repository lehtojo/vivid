import large_function()

pack Object {
	x: large
	y: large
}

pack Foo {
	a: Object
	b: Object
}

Bar {
	a: Object
}

pack Option<T> {
	value: T
	empty: bool

	init(value: T) {
		this.value = value
		this.empty = false
	}

	init() {
		this.empty = true
	}
}

export sum(object: Object) {
	object.x = 2
	=> object.x + object.y
}

export sum(a: Object, b: Object) {
	a = b
	=> a.x + a.y
}

export sum(a: large, b: Object) {
	b.x += a
	=> b
}

export peepeepoopoo(a: large, b: Object) {
	b.x = a
	large_function()
	=> b
}

export get(a: large, b: large) {
	if a > b {
		=> Option<large>(a)
	}

	=> Option<large>()
}

export create_option() {
	=> Option<large>(1)
}

export get_value(from: Option<large>) {
	=> from.value
}

export pass_option() {
	=> get_value(create_option())
}

export return_disposable_pack_value() {
	=> create_option()
}

export save_pack_to_pointer(pointer: link<Object>, i: large) {
	object = Object()
	pointer[i] = object

	=> i + i++
}

export pack_to_pack(foo: Foo) {
	=> foo.a.x
}

export switch_packs(a: Foo, b: Foo) {
	pack_to_pack(b)
	=> a.a.y
}

export create_foo() {
	foo = Foo()
	foo.b.y = 1
	=> foo
}

export receive_foo() {
	foo = create_foo()
	=> foo.b.y
}

export write_pack_to_pointer_type(bar: Bar) {
	bar.a = Object()
}

f(x) {
	=> x * 2
}

g(x) {
	=> x > 2
}

h(x) {
	=> x as large
}

export extract_1(destination: link, x: large) {
	destination[f(x)] = destination[0] + f(x)
}

export extract_2(memory: link, x: large) {
	=> memory[0] + h(g(f(x)) and g(f(f(x))))
}

init() {
	object = Object()
	object.x = 1
	object.y = 2

	object.x = object.x + object.y

	sum(object)
	=> 1
}