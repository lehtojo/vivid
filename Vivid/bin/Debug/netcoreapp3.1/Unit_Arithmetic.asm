.section .text
.intel_syntax noprefix
.file 1 "Sandbox.v"
.global main
main:
jmp _V4initv_rx

.extern _V17internal_allocatex_rPh

.global _V10arithmeticxxx_rx
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

.global _V8additionxx_rx
_V8additionxx_rx:
lea rax, [rcx+rdx]
ret

.global _V11subtractionxx_rx
_V11subtractionxx_rx:
sub rcx, rdx
mov rax, rcx
ret

.global _V14multiplicationxx_rx
_V14multiplicationxx_rx:
imul rcx, rdx
mov rax, rcx
ret

.global _V8divisionxx_rx
_V8divisionxx_rx:
mov rax, rcx
mov r8, rdx
cqo
idiv r8
ret

.global _V22addition_with_constantx_rx
_V22addition_with_constantx_rx:
mov rax, 20
add rax, rcx
ret

.global _V25subtraction_with_constantx_rx
_V25subtraction_with_constantx_rx:
mov rax, -20
add rax, rcx
ret

.global _V28multiplication_with_constantx_rx
_V28multiplication_with_constantx_rx:
imul rax, rcx, 100
ret

.global _V22division_with_constantx_rx
_V22division_with_constantx_rx:
mov rax, 100
cqo
idiv rcx
mov rcx, 1844674407370955162
mul rcx
mov rax, rdx
sar rax, 63
add rax, rdx
ret

.global _V12preincrementx_rx
_V12preincrementx_rx:
lea rax, [rcx+8]
ret

.global _V12predecrementx_rx
_V12predecrementx_rx:
lea rax, [rcx+6]
ret

.global _V13postincrementx_rx
_V13postincrementx_rx:
lea rax, [rcx+3]
ret

.global _V13postdecrementx_rx
_V13postdecrementx_rx:
lea rax, [rcx+3]
ret

.global _V10incrementsx_rx
_V10incrementsx_rx:
mov rdx, rcx
add rcx, 1
add rcx, 1
imul rdx, rcx
lea rax, [rcx+rdx]
add rax, rcx
ret

.global _V10decrementsx_rx
_V10decrementsx_rx:
mov rdx, rcx
sub rcx, 1
sub rcx, 1
imul rdx, rcx
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

_V8allocatex_rPh:
push rbx
push rsi
sub rsp, 40
mov r8, [rip+_VN10Allocation_current]
test r8, r8
je _V8allocatex_rPh_L0
mov rdx, [r8+16]
lea r9, [rdx+rcx]
cmp r9, 1000000
jg _V8allocatex_rPh_L0
lea r9, [rdx+rcx]
mov qword ptr [r8+16], r9
lea r9, [rdx+rcx]
mov rax, [r8+8]
add rax, rdx
add rsp, 40
pop rsi
pop rbx
ret
_V8allocatex_rPh_L0:
mov rbx, rcx
mov rcx, 1000000
call _V17internal_allocatex_rPh
mov rcx, 24
mov rsi, rax
call _V17internal_allocatex_rPh
mov qword ptr [rax+8], rsi
mov qword ptr [rax+16], rbx
mov qword ptr [rip+_VN10Allocation_current], rax
mov rax, rsi
add rsp, 40
pop rsi
pop rbx
ret

_V8inheritsPhS__rx:
push rbx
push rsi
mov r8, [rcx]
mov r9, [rdx]
movzx r10, byte ptr [r9]
xor rax, rax
_V8inheritsPhS__rx_L1:
_V8inheritsPhS__rx_L0:
movzx rcx, byte ptr [r8+rax]
add rax, 1
cmp rcx, r10
jnz _V8inheritsPhS__rx_L4
mov r11, rcx
mov rbx, 1
_V8inheritsPhS__rx_L7:
_V8inheritsPhS__rx_L6:
movzx r11, byte ptr [r8+rax]
movzx rsi, byte ptr [r9+rbx]
add rax, 1
add rbx, 1
cmp r11, rsi
jz _V8inheritsPhS__rx_L9
cmp r11, 1
jne _V8inheritsPhS__rx_L9
test rsi, rsi
jne _V8inheritsPhS__rx_L9
mov rax, 1
pop rsi
pop rbx
ret
_V8inheritsPhS__rx_L9:
jmp _V8inheritsPhS__rx_L6
_V8inheritsPhS__rx_L8:
jmp _V8inheritsPhS__rx_L3
_V8inheritsPhS__rx_L4:
cmp rcx, 2
jne _V8inheritsPhS__rx_L3
xor rax, rax
pop rsi
pop rbx
ret
_V8inheritsPhS__rx_L3:
jmp _V8inheritsPhS__rx_L0
_V8inheritsPhS__rx_L2:
pop rsi
pop rbx
ret

.section .data

_VN10Allocation_current:
.quad 0

_VN4Page_configuration:
.quad _VN4Page_descriptor

_VN4Page_descriptor:
.quad _VN4Page_descriptor_0
.long 24
.long 0

_VN4Page_descriptor_0:
.ascii "Page"
.byte 0
.byte 1
.byte 2
.byte 0

_VN10Allocation_configuration:
.quad _VN10Allocation_descriptor

_VN10Allocation_descriptor:
.quad _VN10Allocation_descriptor_0
.long 8
.long 0

_VN10Allocation_descriptor_0:
.ascii "Allocation"
.byte 0
.byte 1
.byte 2
.byte 0