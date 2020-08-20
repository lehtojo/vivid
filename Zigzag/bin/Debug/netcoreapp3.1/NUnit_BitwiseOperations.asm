section .text
global _start
_start:
call _V4initv_rx
mov rax, 60
xor rdi, rdi
syscall

extern _V8allocatex_rPh

global _V11bitwise_andcc_rc
_V11bitwise_andcc_rc:
and rdi, rsi
mov rax, rdi
ret

global _V11bitwise_xorcc_rc
_V11bitwise_xorcc_rc:
xor rdi, rsi
mov rax, rdi
ret

global _V10bitwise_orcc_rc
_V10bitwise_orcc_rc:
or rdi, rsi
mov rax, rdi
ret

global _V13synthetic_andcc_rc
_V13synthetic_andcc_rc:
mov rax, rdi
xor rax, rsi
not rax
or rdi, rsi
not rdi
xor rax, rdi
ret

global _V13synthetic_xorcc_rc
_V13synthetic_xorcc_rc:
mov rax, rdi
or rax, rsi
and rdi, rsi
not rdi
and rax, rdi
ret

global _V12synthetic_orcc_rc
_V12synthetic_orcc_rc:
mov rax, rdi
xor rax, rsi
and rdi, rsi
xor rax, rdi
ret

global _V18assign_bitwise_andx_rx
_V18assign_bitwise_andx_rx:
mov rcx, rdi
sar rcx, 1
and rdi, rcx
mov rax, rdi
ret

global _V18assign_bitwise_xorx_rx
_V18assign_bitwise_xorx_rx:
xor rdi, 1
mov rax, rdi
ret

global _V17assign_bitwise_orxx_rx
_V17assign_bitwise_orxx_rx:
or rdi, rsi
mov rax, rdi
ret

_V4initv_rx:
sub rsp, 8
mov rax, 1
add rsp, 8
ret
xor rdi, rdi
xor rsi, rsi
call _V11bitwise_andcc_rc
xor rdi, rdi
xor rsi, rsi
call _V11bitwise_xorcc_rc
xor rdi, rdi
xor rsi, rsi
call _V10bitwise_orcc_rc
xor rdi, rdi
xor rsi, rsi
call _V13synthetic_andcc_rc
xor rdi, rdi
xor rsi, rsi
call _V13synthetic_xorcc_rc
xor rdi, rdi
xor rsi, rsi
call _V12synthetic_orcc_rc
xor rdi, rdi
call _V18assign_bitwise_andx_rx
xor rdi, rdi
call _V18assign_bitwise_xorx_rx
xor rdi, rdi
xor rsi, rsi
call _V17assign_bitwise_orxx_rx
ret