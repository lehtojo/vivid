.section .text
.intel_syntax noprefix
.file 1 "Sandbox.v"
.global main
main:
jmp _V4initv_rx

.extern _V17internal_allocatex_rPh

.global _V11bitwise_andcc_rc
_V11bitwise_andcc_rc:
and rcx, rdx
mov rax, rcx
ret

.global _V11bitwise_xorcc_rc
_V11bitwise_xorcc_rc:
xor rcx, rdx
mov rax, rcx
ret

.global _V10bitwise_orcc_rc
_V10bitwise_orcc_rc:
or rcx, rdx
mov rax, rcx
ret

.global _V13synthetic_andcc_rc
_V13synthetic_andcc_rc:
mov rax, rcx
xor rax, rdx
not rax
or rcx, rdx
not rcx
xor rax, rcx
ret

.global _V13synthetic_xorcc_rc
_V13synthetic_xorcc_rc:
mov rax, rcx
or rax, rdx
and rcx, rdx
not rcx
and rax, rcx
ret

.global _V12synthetic_orcc_rc
_V12synthetic_orcc_rc:
mov rax, rcx
xor rax, rdx
and rcx, rdx
xor rax, rcx
ret

.global _V18assign_bitwise_andx_rx
_V18assign_bitwise_andx_rx:
mov rdx, rcx
sar rdx, 1
and rcx, rdx
mov rax, rcx
ret

.global _V18assign_bitwise_xorx_rx
_V18assign_bitwise_xorx_rx:
xor rcx, 1
mov rax, rcx
ret

.global _V17assign_bitwise_orxx_rx
_V17assign_bitwise_orxx_rx:
or rcx, rdx
mov rax, rcx
ret

_V4initv_rx:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
xor rcx, rcx
xor rdx, rdx
call _V11bitwise_andcc_rc
xor rcx, rcx
xor rdx, rdx
call _V11bitwise_xorcc_rc
xor rcx, rcx
xor rdx, rdx
call _V10bitwise_orcc_rc
xor rcx, rcx
xor rdx, rdx
call _V13synthetic_andcc_rc
xor rcx, rcx
xor rdx, rdx
call _V13synthetic_xorcc_rc
xor rcx, rcx
xor rdx, rdx
call _V12synthetic_orcc_rc
xor rcx, rcx
call _V18assign_bitwise_andx_rx
xor rcx, rcx
call _V18assign_bitwise_xorx_rx
xor rcx, rcx
xor rdx, rdx
call _V17assign_bitwise_orxx_rx
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