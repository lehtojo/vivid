import large_function()

OS = 'Windows'

true = 1
false = 0
none = 0

outline arithmetic(a, b, c, d, e) {
	x = a + b
	y = a - b
	z = a * b
	w = a / b
	i = a % b

	x += 2 * a
	y -= 3 * b
	z *= 5 * c
	w /= 7 * d
	i %= 11 * e

	# a <| b
	# b |> a
	
	=> x + y + z + w + i
}

outline bitwise_operations(a, b) {
	a &= b
	b |= a
	a ¤= b

	=> !(a & b | a ¤ b)
}

outline conditionals(a, b) {
	if a >= -1 and a <= 1 {
		=> arithmetic(a, a + 1, a + 2, a + 3, a + 4)
	}
	else b < -1 or b > 1 {
		=> arithmetic(b - 4, b - 3, b + 2, b + 1, b)
	}
	else a == b {
		large_function()
		=> 0
	}
}

outline loops(a, b) {
	r = 0

	loop {
		r += a
		large_function()

		if r > 2020 {
			stop
		}
	}

	loop (r > 0) {
		r--
		conditionals(a, b)
	}

	loop (i = 0, i < b, i++) {
		if a == 0 or b == 0 {
			continue
		}

		r -= bitwise_operations(a - b, b - a)
	}

	=> r
}

outline increments(a) {
	=> ++a + ++a + a++ + a++ + a++ + ++a + ++a + a++
}

outline decrements(b) {
	=> --b - --b - b-- - b-- - b-- - --b - --b - b--
}

if OS == 'Windows' {
	MESSAGE = 'Hello Windows!'
}
else OS == 'MacOS' {
	MESSAGE = 'Hello MacOS!'
}
else OS == 'Linux' {
	MESSAGE = 'Hello Linux!'
}
else {
	MESSAGE = 'Hello OS!'
}

Inheritant {
	private:
	a = 0u8
	b: tiny = 0

	c = 1u16
	d: small = 1

	protected:
	i: u32 = 7
	j = 7i32

	k: u64 = -11
	l = -11i64

	public:
	x: bool
	y: link = MESSAGE

	q = 0.0

	init(x) {
		this.x = x
	}

	deinit() {
		a = 0
		b = 0
		c = 0
		d = 0
		i = 0
		j = 0
		k = 0
		l = 0
		x = 0
		y = 0
		q = 0.0
	}
}

Box<X> {
	private:
	items: link
	size: num
	position = 0

	public:
	init(size: num) {
		items = allocate(X.size * size)
		this.size = size
	}

	put(item: X) {
		items[position * X.size] = item
		position++
	}

	get(i: num) {
		=> items[i * X.size] as X
	}

	set(i: num, value) {
		items[i * X.size] = value as X
	}

	clear() {
		loop (position >= 0, position--) {
			this[position] = 0
		}
	}

	size() => size
	is_full() => position >= size
}

Inheritant Inheritor<X, Y> {
	private:
	box: Box<X>
	filter: (X) => bool

	public:
	init(capacity: num, filter: (X) => bool) {
		this.filter = filter
		box = Box<X>(capacity)
	}

	configure(with: Y) {
		i = -with
		j = +with
		k = -with
		l = +with
		x = -with
		y = +with
	}

	put(item: X) {
		if item.worth() < 10 or filter(item) == false {
			return
		}

		box.put(item)
	}

	remove(item: X) {
		if (box.is_full()) {
			return
		}

		loop (i = 0, i < box.size(), i++) {
			if box[i] == item {
				box[i] = none
				stop
			}
		}
	}

	clear() {
		box.clear()
	}

	assign_plus(item: Banana) {
		put(item)
	}

	assign_minus(item: Banana) {
		remove(item)
	}
}

Banana {
	stinkiness = 0

	worth() => stinkiness * stinkiness
}

Inheritant.assign_times(q: decimal) {
	this.q *= q

	if this.q < 0 {
		this.q = -this.q
	}
}

Inheritant.assign_divide(q: decimal) {
	this.q /= q

	if this.q < 0 {
		this.q = -this.q
	}
}

#Inheritant.to<T>() => this as T
#Inheritor.to<T>() => this as T

init() {
	x = arithmetic(1, 2, 3, 4, 5)
	y = arithmetic(-1.0, -2.0, -3.0, -4.0, -5.0)

	bitwise_operations(bitwise_operations(1, 0), bitwise_operations(0, 1))

	conditionals(x, y)

	loops(20, 20)

	increments(1)
	decrements(1)

	a = Inheritor<Banana, normal>(7, (i: Banana) => i.stinkiness > 999)
	a.configure(-1)
	
	banana = Banana()
	banana.stinkiness = -999999

	a += banana
	a -= banana

	a *= -3.14159 + 1.41421 - -3.14159 * 1.41421 / -3.14159
	a /= 3.14159 + -1.41421 - 3.14159 * -1.41421 / 3.14159

	#a.to(Inheritant).to(Inheritor).clear()
	a.clear()

	=> true
}