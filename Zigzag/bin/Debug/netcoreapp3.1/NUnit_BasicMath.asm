section .text
global _start
_start:
call _V4initv_rx
mov rax, 60
xor rdi, rdi
syscall

extern _V8allocatex_rPh

global _V10basic_mathxxx_rx
_V10basic_mathxxx_rx:
mov rax, rdi
imul rax, rdx
add rax, rdi
add rax, rdx
imul rsi, rdi
add rdx, 1
imul rsi, rdx
imul rsi, 100
add rax, rsi
ret

global _V8additionxx_rx
_V8additionxx_rx:
add rdi, rsi
mov rax, rdi
ret

global _V11subtractionxx_rx
_V11subtractionxx_rx:
sub rdi, rsi
mov rax, rdi
ret

global _V14multiplicationxx_rx
_V14multiplicationxx_rx:
imul rdi, rsi
mov rax, rdi
ret

global _V8divisionxx_rx
_V8divisionxx_rx:
mov rax, rdi
xor rdx, rdx
idiv rsi
ret

global _V22addition_with_constantx_rx
_V22addition_with_constantx_rx:
mov rax, 10
add rax, rdi
add rax, 10
ret

global _V25subtraction_with_constantx_rx
_V25subtraction_with_constantx_rx:
mov rax, -10
add rax, rdi
sub rax, 10
ret

global _V28multiplication_with_constantx_rx
_V28multiplication_with_constantx_rx:
mov rax, 10
imul rax, rdi
imul rax, 10
ret

global _V22division_with_constantx_rx
_V22division_with_constantx_rx:
mov rax, 100
xor rdx, rdx
idiv rdi
mov rcx, 1844674407370955162
mul rcx
mov rax, rdx
sar rax, 63
add rax, rdx
ret

_V4initv_rx:
sub rsp, 8
mov rax, 1
add rsp, 8
ret
xor rdi, rdi
xor rsi, rsi
call _V8additionxx_rx
xor rdi, rdi
xor rsi, rsi
call _V11subtractionxx_rx
xor rdi, rdi
xor rsi, rsi
call _V14multiplicationxx_rx
mov rdi, 1
mov rsi, 1
call _V8divisionxx_rx
xor rdi, rdi
call _V22addition_with_constantx_rx
xor rdi, rdi
call _V25subtraction_with_constantx_rx
xor rdi, rdi
call _V28multiplication_with_constantx_rx
xor rdi, rdi
call _V22division_with_constantx_rx
mov rdi, 1
mov rsi, 2
mov rdx, 3
call _V10basic_mathxxx_rx
ret