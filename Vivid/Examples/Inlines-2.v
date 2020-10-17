import large_function()

call() {
	large_function()
}

outline inlines_call() {
	call()
}

sum(a, b) {
	=> a + b
}

outline inlines_call_with_value(x) {
	=> (x + 1) * sum(x, -1)
}

foo(x) {
	=> x + 10
}

bar(x) {
	=> x - 7
}

baz(x) {
	=> foo(x) * bar(x)
}

outline inlines_nested_calls(x, y) {
	=> baz(x) * baz(y)
}

Player {
	private health: num

	init(health) {
		this.health = health
	}

	get_health() {
		=> health
	}
}

create_player(health: num) {
	=> Player(health)
}

outline inlines_dependent_functions(x) {
	=> create_player(x).get_health()
}

Numbers {
	private:   
	memory: link
	capacity: num

	public:

	init(capacity) {
		this.capacity = capacity
		this.memory = allocate(capacity * 8)
	}

	set(i: num, value: num) {
		memory[i * 8] = value
	}

	fill(value: num) {
		loop (i = 0, i < capacity, ++i) {
			set(i, value)
		}
	}
}

outline inlines_internal_member_calls(x) {
	numbers = Numbers(x)
	numbers.fill(x / 2)
}

init() {
	inlines_call()
	inlines_call_with_value(0)
	inlines_nested_calls(0, 0)
	inlines_dependent_functions(0)
	inlines_internal_member_calls(0)
}