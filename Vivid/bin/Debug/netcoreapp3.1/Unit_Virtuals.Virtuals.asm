.section .text
.intel_syntax noprefix
.global main
main:
jmp _V4initv_rx

.extern _V4copyPhxS_
.extern _V11offset_copyPhxS_x
.extern _V14internal_printPhx
.extern _V17internal_allocatex_rPh

.global _VN14VirtualTypeOne3fooEv
_VN14VirtualTypeOne3fooEv_v:
_VN14VirtualTypeOne3fooEv:
sub rsp, 40
mov rcx, 3
call _V7printlnx
add rsp, 40
ret

.global _VN14VirtualTypeTwo3barEv
_VN14VirtualTypeTwo3barEv_v:
_VN14VirtualTypeTwo3barEv:
sub rsp, 40
mov rdx, [rcx+8]
imul rdx, rdx
movsd xmm0, qword ptr [rcx+24]
mulsd xmm0, xmm0
cvtsi2sd xmm1, rdx
addsd xmm1, xmm0
movsd xmm0, xmm1
call _V7printlnd
add rsp, 40
ret

.global _VN16VirtualTypeThree3bazEcs_rx
_VN16VirtualTypeThree3bazEcs_rx_v:
_VN16VirtualTypeThree3bazEcs_rx:
push rbx
push rsi
push rdi
sub rsp, 32
mov rbx, rdx
mov rsi, r8
mov rdi, rcx
cmp rbx, rsi
jle _VN16VirtualTypeThree3bazEcs_rx_L1
mov rcx, rbx
call _V7printlnc
mov rax, rbx
add rsp, 32
pop rdi
pop rsi
pop rbx
ret
mov rbx, rax
jmp _VN16VirtualTypeThree3bazEcs_rx_L0
_VN16VirtualTypeThree3bazEcs_rx_L1:
cmp rsi, rbx
jle _VN16VirtualTypeThree3bazEcs_rx_L3
mov rcx, rsi
call _V7printlns
mov rax, rsi
add rsp, 32
pop rdi
pop rsi
pop rbx
ret
mov rsi, rax
jmp _VN16VirtualTypeThree3bazEcs_rx_L0
_VN16VirtualTypeThree3bazEcs_rx_L3:
movsd xmm0, qword ptr [rdi+24]
call _V7printlnd
cvttsd2si rax, [rdi+24]
add rsp, 32
pop rdi
pop rsi
pop rbx
ret
_VN16VirtualTypeThree3bazEcs_rx_L0:
add rsp, 32
pop rdi
pop rsi
pop rbx
ret

.global _VN15VirtualTypeFour3fooEv
_VN15VirtualTypeFour3fooEv_v:
_VN15VirtualTypeFour3fooEv:
add qword ptr [rcx+16], 1
sub qword ptr [rcx+32], 1
ret

.global _VN15VirtualTypeFour3barEv
_VN15VirtualTypeFour3barEv_v:
sub rcx, 8
_VN15VirtualTypeFour3barEv:
mov rdx, [rcx+16]
imul rdx, 7
mov qword ptr [rcx+16], rdx
mov rdx, [rcx+32]
imul rdx, 7
mov qword ptr [rcx+32], rdx
ret

.global _VN15VirtualTypeFour3bazEcs_rx
_VN15VirtualTypeFour3bazEcs_rx_v:
sub rcx, 24
_VN15VirtualTypeFour3bazEcs_rx:
mov rax, [rcx+16]
mov r9, rdx
cqo
idiv qword ptr [rcx+32]
mov rcx, rax
mov rax, r9
cqo
idiv r8
add rcx, rax
mov rax, rcx
ret

.global _VN14VirtualTypeOne4initEv_rPS_
_VN14VirtualTypeOne4initEv_rPS_:
sub rsp, 40
mov rcx, 16
call _V8allocatex_rPh
lea rcx, [rip+_VN14VirtualTypeOne_configuration+8]
mov qword ptr [rax], rcx
add rsp, 40
ret

.global _VN14VirtualTypeTwo4initEv_rPS_
_VN14VirtualTypeTwo4initEv_rPS_:
sub rsp, 40
mov rcx, 32
call _V8allocatex_rPh
lea rcx, [rip+_VN14VirtualTypeTwo_configuration+8]
mov qword ptr [rax], rcx
add rsp, 40
ret

.global _VN16VirtualTypeThree4initEv_rPS_
_VN16VirtualTypeThree4initEv_rPS_:
sub rsp, 40
mov rcx, 32
call _V8allocatex_rPh
lea rcx, [rip+_VN16VirtualTypeThree_configuration+8]
mov qword ptr [rax], rcx
add rsp, 40
ret

.global _VN15VirtualTypeFour4initEv_rPS_
_VN15VirtualTypeFour4initEv_rPS_:
sub rsp, 40
mov rcx, 48
call _V8allocatex_rPh
lea rcx, [rip+_VN15VirtualTypeFour_configuration+40]
mov qword ptr [rax+24], rcx
lea rcx, [rip+_VN15VirtualTypeFour_configuration+24]
mov qword ptr [rax+8], rcx
lea rcx, [rip+_VN15VirtualTypeFour_configuration+8]
mov qword ptr [rax], rcx
add rsp, 40
ret

.global _V24execute_virtual_type_onev
_V24execute_virtual_type_onev:
push rbx
sub rsp, 32
call _VN14VirtualTypeOne4initEv_rPS_
mov rcx, rax
mov rbx, rax
call _VN14VirtualTypeOne3fooEv
mov rcx, rbx
mov rdx, [rbx]
call qword ptr [rdx+8]
add rsp, 32
pop rbx
ret

.global _V24execute_virtual_type_twov
_V24execute_virtual_type_twov:
push rbx
sub rsp, 32
call _VN14VirtualTypeTwo4initEv_rPS_
mov qword ptr [rax+8], 7
mov rcx, 4631107791820423168
mov qword ptr [rax+24], rcx
mov rcx, rax
mov rbx, rax
call _VN14VirtualTypeTwo3barEv
mov rcx, rbx
mov rdx, [rbx]
call qword ptr [rdx+8]
add rsp, 32
pop rbx
ret

.global _V26execute_virtual_type_threev
_V26execute_virtual_type_threev:
push rbx
sub rsp, 32
call _VN16VirtualTypeThree4initEv_rPS_
mov qword ptr [rax+8], 1
mov rcx, 4621819117588971520
mov qword ptr [rax+24], rcx
mov rcx, rax
mov rdx, 1
mov r8, -1
mov rbx, rax
call _VN16VirtualTypeThree3bazEcs_rx
mov rcx, rax
call _V7printlnx
mov rcx, rbx
mov rdx, 255
mov r8, 32767
mov r9, [rbx]
call qword ptr [r9+8]
mov rcx, rax
call _V7printlnx
mov rcx, rbx
mov rdx, 7
mov r8, 7
mov r9, [rbx]
call qword ptr [r9+8]
mov rcx, rax
call _V7printlnx
add rsp, 32
pop rbx
ret

.global _V25execute_virtual_type_fourv
_V25execute_virtual_type_fourv:
push rbx
sub rsp, 32
call _VN15VirtualTypeFour4initEv_rPS_
mov qword ptr [rax+16], -6942
mov qword ptr [rax+32], 4269
mov rcx, rax
mov rbx, rax
call _VN15VirtualTypeFour3fooEv
mov rcx, rbx
call _VN15VirtualTypeFour3barEv
mov rcx, rbx
mov rdx, 64
mov r8, 8
call _VN15VirtualTypeFour3bazEcs_rx
mov rcx, rax
call _V7printlnx
mov rcx, rbx
mov rdx, [rbx]
call qword ptr [rdx+8]
lea rcx, [rbx+8]
lea r8, [rbx+8]
mov rdx, [r8]
call qword ptr [rdx+8]
lea rcx, [rbx+24]
xor rdx, rdx
mov r8, 1
lea r10, [rbx+24]
mov r9, [r10]
call qword ptr [r9+8]
mov rcx, rax
call _V7printlnx
add rsp, 32
pop rbx
ret

.global _V4initv_rx
_V4initv_rx:
sub rsp, 40
call _V24execute_virtual_type_onev
call _V24execute_virtual_type_twov
call _V26execute_virtual_type_threev
call _V25execute_virtual_type_fourv
xor rax, rax
add rsp, 40
ret

.section .data

_VN13InheritantOne_configuration:
.quad _VN13InheritantOne_descriptor

_VN13InheritantOne_descriptor:
.quad _VN13InheritantOne_descriptor_0
.long 8
.long 0

_VN13InheritantOne_descriptor_0:
.ascii "InheritantOne"
.byte 0
.byte 1
.byte 2
.byte 0

_VN14VirtualTypeOne_configuration:
.quad _VN14VirtualTypeOne_descriptor
.quad _VN14VirtualTypeOne_descriptor
.quad _VN14VirtualTypeOne3fooEv_v

_VN14VirtualTypeOne_descriptor:
.quad _VN14VirtualTypeOne_descriptor_0
.long 16
.long 1
.quad _VN13InheritantOne_descriptor

_VN14VirtualTypeOne_descriptor_0:
.ascii "VirtualTypeOne"
.byte 0
.byte 1
.ascii "InheritantOne"
.byte 1
.byte 2
.byte 0

_VN13InheritantTwo_configuration:
.quad _VN13InheritantTwo_descriptor

_VN13InheritantTwo_descriptor:
.quad _VN13InheritantTwo_descriptor_0
.long 16
.long 0

_VN13InheritantTwo_descriptor_0:
.ascii "InheritantTwo"
.byte 0
.byte 1
.byte 2
.byte 0

_VN14VirtualTypeTwo_configuration:
.quad _VN14VirtualTypeTwo_descriptor
.quad _VN14VirtualTypeTwo_descriptor
.quad _VN14VirtualTypeTwo3barEv_v

_VN14VirtualTypeTwo_descriptor:
.quad _VN14VirtualTypeTwo_descriptor_0
.long 32
.long 1
.quad _VN13InheritantTwo_descriptor

_VN14VirtualTypeTwo_descriptor_0:
.ascii "VirtualTypeTwo"
.byte 0
.byte 1
.ascii "InheritantTwo"
.byte 1
.byte 2
.byte 0

_VN15InheritantThree_configuration:
.quad _VN15InheritantThree_descriptor

_VN15InheritantThree_descriptor:
.quad _VN15InheritantThree_descriptor_0
.long 16
.long 0

_VN15InheritantThree_descriptor_0:
.ascii "InheritantThree"
.byte 0
.byte 1
.byte 2
.byte 0

_VN16VirtualTypeThree_configuration:
.quad _VN16VirtualTypeThree_descriptor
.quad _VN16VirtualTypeThree_descriptor
.quad _VN16VirtualTypeThree3bazEcs_rx_v

_VN16VirtualTypeThree_descriptor:
.quad _VN16VirtualTypeThree_descriptor_0
.long 32
.long 1
.quad _VN15InheritantThree_descriptor

_VN16VirtualTypeThree_descriptor_0:
.ascii "VirtualTypeThree"
.byte 0
.byte 1
.ascii "InheritantThree"
.byte 1
.byte 2
.byte 0

_VN15VirtualTypeFour_configuration:
.quad _VN15VirtualTypeFour_descriptor
.quad _VN15VirtualTypeFour_descriptor
.quad _VN15VirtualTypeFour3fooEv_v
.quad _VN15VirtualTypeFour_descriptor
.quad _VN15VirtualTypeFour3barEv_v
.quad _VN15VirtualTypeFour_descriptor
.quad _VN15VirtualTypeFour3bazEcs_rx_v

_VN15VirtualTypeFour_descriptor:
.quad _VN15VirtualTypeFour_descriptor_0
.long 48
.long 3
.quad _VN13InheritantOne_descriptor
.quad _VN13InheritantTwo_descriptor
.quad _VN15InheritantThree_descriptor

_VN15VirtualTypeFour_descriptor_0:
.ascii "VirtualTypeFour"
.byte 0
.byte 1
.ascii "InheritantOne"
.byte 1
.ascii "InheritantTwo"
.byte 1
.ascii "InheritantThree"
.byte 1
.byte 2
.byte 0

