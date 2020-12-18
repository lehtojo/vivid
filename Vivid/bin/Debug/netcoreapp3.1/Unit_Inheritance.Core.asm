.section .text
.intel_syntax noprefix
.global _V8allocatex_rPh
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

.global _V8inheritsPhS__rc
_V8inheritsPhS__rc:
push rbx
push rsi
mov r8, [rcx]
mov r9, [rdx]
movsx r10, byte ptr [r9]
xor rax, rax
_V8inheritsPhS__rc_L1:
_V8inheritsPhS__rc_L0:
movsx rcx, byte ptr [r8+rax]
add rax, 1
cmp rcx, r10
jne _V8inheritsPhS__rc_L4
mov r11, rcx
mov rbx, 1
_V8inheritsPhS__rc_L7:
_V8inheritsPhS__rc_L6:
movsx r11, byte ptr [r8+rax]
movsx rsi, byte ptr [r9+rbx]
add rax, 1
add rbx, 1
cmp r11, rsi
je _V8inheritsPhS__rc_L9
cmp r11, 1
jne _V8inheritsPhS__rc_L9
test rsi, rsi
jne _V8inheritsPhS__rc_L9
mov rax, 1
pop rsi
pop rbx
ret
_V8inheritsPhS__rc_L9:
jmp _V8inheritsPhS__rc_L6
_V8inheritsPhS__rc_L8:
jmp _V8inheritsPhS__rc_L3
_V8inheritsPhS__rc_L4:
cmp rcx, 2
jne _V8inheritsPhS__rc_L3
xor rax, rax
pop rsi
pop rbx
ret
_V8inheritsPhS__rc_L3:
jmp _V8inheritsPhS__rc_L0
_V8inheritsPhS__rc_L2:
pop rsi
pop rbx
ret

.section .data

_VN10Allocation_current: .quad 0

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

