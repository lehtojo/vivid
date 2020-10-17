section .text
global main
main:
jmp _V4initv_rx

extern _V8allocatex_rPh

global _V10arithmeticxxx_rx
export _V10arithmeticxxx_rx
_V10arithmeticxxx_rx:
mov rax, rcx
imul rax, r8
add rax, rcx
add rax, r8
imul rdx, rcx
add r8, 1
imul rdx, r8
imul rdx, 100
add rax, rdx
ret

global _V8additionxx_rx
export _V8additionxx_rx
_V8additionxx_rx:
add rcx, rdx
mov rax, rcx
ret

global _V11subtractionxx_rx
export _V11subtractionxx_rx
_V11subtractionxx_rx:
sub rcx, rdx
mov rax, rcx
ret

global _V14multiplicationxx_rx
export _V14multiplicationxx_rx
_V14multiplicationxx_rx:
imul rcx, rdx
mov rax, rcx
ret

global _V8divisionxx_rx
export _V8divisionxx_rx
_V8divisionxx_rx:
mov rax, rcx
mov r8, rdx
xor rdx, rdx
idiv r8
ret

global _V22addition_with_constantx_rx
export _V22addition_with_constantx_rx
_V22addition_with_constantx_rx:
mov rax, 20
add rax, rcx
ret

global _V25subtraction_with_constantx_rx
export _V25subtraction_with_constantx_rx
_V25subtraction_with_constantx_rx:
mov rax, -20
add rax, rcx
ret

global _V28multiplication_with_constantx_rx
export _V28multiplication_with_constantx_rx
_V28multiplication_with_constantx_rx:
imul rcx, 100
mov rax, rcx
ret

global _V22division_with_constantx_rx
export _V22division_with_constantx_rx
_V22division_with_constantx_rx:
mov rax, 100
xor rdx, rdx
idiv rcx
mov rcx, 1844674407370955162
mul rcx
mov rax, rdx
sar rax, 63
add rax, rdx
ret

global _V12preincrementx_rx
export _V12preincrementx_rx
_V12preincrementx_rx:
mov rax, 14
ret

global _V12predecrementx_rx
export _V12predecrementx_rx
_V12predecrementx_rx:
mov rax, 14
ret

global _V13postincrementx_rx
export _V13postincrementx_rx
_V13postincrementx_rx:
mov rax, 6
ret

global _V13postdecrementx_rx
export _V13postdecrementx_rx
_V13postdecrementx_rx:
mov rax, 6
ret

global _V10incrementsx_rx
export _V10incrementsx_rx
_V10incrementsx_rx:
mov rdx, rcx
add rcx, 1
add rcx, 1
mov r8, rcx
imul rdx, r8
lea rax, [rcx+rdx]
add rax, rcx
ret

global _V10decrementsx_rx
export _V10decrementsx_rx
_V10decrementsx_rx:
mov rdx, rcx
sub rcx, 1
sub rcx, 1
mov r8, rcx
imul rdx, r8
lea rax, [rcx+rdx]
add rax, rcx
ret

_V4initv_rx:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
xor rcx, rcx
xor rdx, rdx
call _V8additionxx_rx
xor rcx, rcx
xor rdx, rdx
call _V11subtractionxx_rx
xor rcx, rcx
xor rdx, rdx
call _V14multiplicationxx_rx
mov rcx, 1
mov rdx, 1
call _V8divisionxx_rx
xor rcx, rcx
call _V22addition_with_constantx_rx
xor rcx, rcx
call _V25subtraction_with_constantx_rx
xor rcx, rcx
call _V28multiplication_with_constantx_rx
xor rcx, rcx
call _V22division_with_constantx_rx
mov rcx, 1
mov rdx, 2
mov r8, 3
call _V10arithmeticxxx_rx
mov rcx, 1
call _V12preincrementx_rx
mov rcx, 1
call _V12predecrementx_rx
mov rcx, 1
call _V13postincrementx_rx
mov rcx, 1
call _V13postdecrementx_rx
mov rcx, 1
call _V10incrementsx_rx
mov rcx, 1
call _V10decrementsx_rx
ret