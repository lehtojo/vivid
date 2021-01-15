Holder {
	a: large
	b: large
	c: large
	d: large
}

export goo(x: Holder, y: Holder) {
	x.a = y.a
	x.b = y.b
	x.c = y.c
	x.d = y.d
}

Integers {
	address: link<large>
	count: large

	init(count) {
		this.address = allocate<large>(count)
		this.count = count
	}

	private init(address: link<large>, count: large) {
		this.address = address
		this.count = count
	}

	get(i: large) => address[i]
	set(i: large, v: large) {
		address[i] = v
	}

	select(mapper: (large) -> large) {
		result = allocate<large>(count)

		loop (i = 0, i < count, i++) {
			result[i] = mapper(address[i])
		}

		=> Integers(result, count)
	}
}

mapper(number: large) => number * 5

init() {
	integers = Integers(3)
	integers[0] = 2
	integers[1] = 4
	integers[2] = 8

	result = integers.select(i -> i * 5)
	result = result.select(mapper)

	loop (i = 0, i < result.count, i++) {
		println(result[i])
	}

	=> true
}