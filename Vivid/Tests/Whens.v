export numerical_when(x: num) {
    => when(x) {
        7 => x * x,
        3 => x + x + x,
        1 => -1,
        else => x
    }
}

init() {
    numerical_when(0)
    => 1
}