section .text
global _start
_start:
call _V4initv_rx
mov rax, 60
xor rdi, rdi
syscall

extern _V8allocatex_rPh

global _V18basic_if_statementxx_rx
_V18basic_if_statementxx_rx:
cmp rdi, rsi
jl _V18basic_if_statementxx_rx_L1
mov rax, rdi
ret
jmp _V18basic_if_statementxx_rx_L0
_V18basic_if_statementxx_rx_L1:
mov rax, rsi
ret
_V18basic_if_statementxx_rx_L0:
ret

_V4initv_rx:
sub rsp, 8
mov rax, 1
add rsp, 8
ret
mov rdi, 1
mov rsi, 2
call _V18basic_if_statementxx_rx
ret