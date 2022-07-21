export numerical_when(x: large) {
	return when(x) {
		7 => x * x
		3 => x + x + x
		1 => -1
		else => x
	}
}

export create_string(characters: link, length: large) {
	return String(characters, length)
}

export string_when(text: String) {
	return when(text) {
		'Foo' => 0
		'Bar' => 1, # The commas are here and down below, since the compiler should not care about them here
		'Baz' => 2
		else => -1,
	}
}

Boo {
	x: large
	y: large
}

Boo Baba {
	init(x) {
		this.x = x
	}

	value() {
		return x * x
	}
}

Boo Bui {
	init(y) {
		this.y = y
	}

	value() {
		return y + y
	}
}

Baba Bababui {
	init(x, y) {
		Baba.init(x)
		this.y = y
	}

	value() {
		return y * Baba.value()
	}
}

export create_boo() {
	return Boo()
}

export create_baba(x: large) {
	return Baba(x)
}
export create_bui(x: large) {
	return Bui(x)
}

export create_bababui(x: large, y: large) {
	return Bababui(x, y)
}

export is_when(object: Boo) {
	return when(object) {
		is Bababui bababui => bababui.value(),
		is Baba baba => baba.value(),
		is Bui bui => bui.value(),
		else => -1
	}
}

export range_when(x: large) {
	return when(x) {
		> 10 => x * x,
		<= -7 => 2 * x,
		else => x
	}
}

init() {
	numerical_when(0)
	boo = create_boo()
	is_when(boo)
	return 1
}