.section .text
.intel_syntax noprefix
.global main
main:
jmp _V4initv_rx

.extern _V4copyPhxS_
.extern _V11offset_copyPhxS_x
.extern _V4sqrtd_rd
.extern _V3powdd_rd
.extern _V14internal_printPhx
.extern _V17internal_allocatex_rPh

.global _VN6Vector6invertEv
_VN6Vector6invertEv:
movsd xmm0, qword ptr [rcx+8]
xorpd xmm0, oword ptr [rip+_VN6Vector6invertEv_C0]
movsd qword ptr [rcx+8], xmm0
movsd xmm0, qword ptr [rcx+16]
xorpd xmm0, oword ptr [rip+_VN6Vector6invertEv_C1]
movsd qword ptr [rcx+16], xmm0
ret

.global _VN6Vector5minusEPS__rS0_
_VN6Vector5minusEPS__rS0_:
sub rsp, 40
movsd xmm0, qword ptr [rcx+8]
subsd xmm0, qword ptr [rdx+8]
movsd xmm1, qword ptr [rcx+16]
subsd xmm1, qword ptr [rdx+16]
call _VN6Vector4initEdd_rPS_
add rsp, 40
ret

.global _VN6Vector5timesEd_rPS_
_VN6Vector5timesEd_rPS_:
sub rsp, 40
movsd xmm1, qword ptr [rcx+8]
mulsd xmm1, xmm0
movsd xmm2, qword ptr [rcx+16]
mulsd xmm2, xmm0
movsd xmm0, xmm1
movsd xmm1, xmm2
call _VN6Vector4initEdd_rPS_
add rsp, 40
ret

.global _VN6Vector5timesEx_rPS_
_VN6Vector5timesEx_rPS_:
sub rsp, 40
movsd xmm0, qword ptr [rcx+8]
cvtsi2sd xmm1, rdx
mulsd xmm0, xmm1
movsd xmm2, qword ptr [rcx+16]
mulsd xmm2, xmm1
movsd xmm1, xmm2
call _VN6Vector4initEdd_rPS_
add rsp, 40
ret

.global _VN6Vector11assign_plusEPS_
_VN6Vector11assign_plusEPS_:
movsd xmm0, qword ptr [rcx+8]
addsd xmm0, qword ptr [rdx+8]
movsd qword ptr [rcx+8], xmm0
movsd xmm0, qword ptr [rcx+16]
addsd xmm0, qword ptr [rdx+16]
movsd qword ptr [rcx+16], xmm0
ret

.global _VN6Vector12assign_timesEx
_VN6Vector12assign_timesEx:
movsd xmm0, qword ptr [rcx+8]
cvtsi2sd xmm1, rdx
mulsd xmm0, xmm1
movsd qword ptr [rcx+8], xmm0
movsd xmm0, qword ptr [rcx+16]
mulsd xmm0, xmm1
movsd qword ptr [rcx+16], xmm0
ret

.global _VN6Animal8interactEPS_
_VN6Animal8interactEPS_:
push rbx
sub rsp, 32
mov r8, [rcx+8]
mov rbx, rcx
mov rcx, r8
call qword ptr [r8]
mov rcx, rbx
mov rdx, rax
call _VN6Animal4moveEP6Vector
add rsp, 32
pop rbx
ret

.global _VN6Animal4moveEP6Vector
_VN6Animal4moveEP6Vector:
sub rsp, 40
mov r8, rcx
mov rcx, [r8+24]
call _VN6Vector11assign_plusEPS_
add rsp, 40
ret

.global _VN3Dog4barkEv
_VN3Dog4barkEv:
sub rsp, 40
lea rcx, [rip+_VN3Dog4barkEv_S0]
call _V7printlnPh
add rsp, 40
ret

.global _VN3Cat4meowEv
_VN3Cat4meowEv:
sub rsp, 40
lea rcx, [rip+_VN3Cat4meowEv_S0]
call _V7printlnPh
add rsp, 40
ret

.global _VN6Vector4initExx_rPS_
_VN6Vector4initExx_rPS_:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rcx, 24
mov rsi, rdx
call _V8allocatex_rPh
cvtsi2sd xmm0, rbx
movsd qword ptr [rax+8], xmm0
cvtsi2sd xmm0, rsi
movsd qword ptr [rax+16], xmm0
add rsp, 40
pop rsi
pop rbx
ret

.global _VN6Vector4initEdd_rPS_
_VN6Vector4initEdd_rPS_:
push rbx
sub rsp, 32
mov rcx, 24
movsd qword ptr [rsp+48], xmm0
movsd qword ptr [rsp+56], xmm1
call _V8allocatex_rPh
mov rcx, 24
mov rbx, rax
call _V8allocatex_rPh
movsd xmm0, qword ptr [rsp+48]
movsd qword ptr [rbx+8], xmm0
movsd xmm0, qword ptr [rsp+56]
movsd qword ptr [rbx+16], xmm0
mov rax, rbx
add rsp, 32
pop rbx
ret

.global _VN3Dog4initEv_rPS_
_VN3Dog4initEv_rPS_:
push rbx
sub rsp, 32
mov rcx, 40
call _V8allocatex_rPh
lea rcx, [rip+_VN3Dog_configuration+8]
mov qword ptr [rax], rcx
xor rcx, rcx
xor rdx, rdx
mov rbx, rax
call _VN6Vector4initExx_rPS_
mov qword ptr [rbx+24], rax
mov qword ptr [rbx+16], 0
mov rcx, 16
call _V8allocatex_rPh
lea rcx, [rip+_VN3Dog4initEv_rPS__0_P6Animal_rP6Vector]
mov qword ptr [rax], rcx
mov qword ptr [rax+8], rbx
mov qword ptr [rbx+8], rax
mov rax, rbx
add rsp, 32
pop rbx
ret

.global _VN3Cat4initEv_rPS_
_VN3Cat4initEv_rPS_:
push rbx
sub rsp, 32
mov rcx, 40
call _V8allocatex_rPh
lea rcx, [rip+_VN3Cat_configuration+8]
mov qword ptr [rax], rcx
xor rcx, rcx
xor rdx, rdx
mov rbx, rax
call _VN6Vector4initExx_rPS_
mov qword ptr [rbx+24], rax
mov qword ptr [rbx+16], 1
mov rcx, 16
call _V8allocatex_rPh
lea rcx, [rip+_VN3Cat4initEv_rPS__0_P6Animal_rP6Vector]
mov qword ptr [rax], rcx
mov qword ptr [rax+8], rbx
mov qword ptr [rbx+8], rax
mov rax, rbx
add rsp, 32
pop rbx
ret

.global _V21create_default_actionv_rPFvE
_V21create_default_actionv_rPFvE:
sub rsp, 40
mov rcx, 8
call _V8allocatex_rPh
lea rcx, [rip+_V21create_default_actionv_rPFvE_0_]
mov qword ptr [rax], rcx
add rsp, 40
ret

.global _V22execute_default_actionPFvE
_V22execute_default_actionPFvE:
sub rsp, 40
call qword ptr [rcx]
add rsp, 40
ret

.global _V20create_number_actionv_rPFvxE
_V20create_number_actionv_rPFvxE:
sub rsp, 40
mov rcx, 8
call _V8allocatex_rPh
lea rcx, [rip+_V20create_number_actionv_rPFvxE_0_x]
mov qword ptr [rax], rcx
add rsp, 40
ret

.global _V21execute_number_actionPFvxEx
_V21execute_number_actionPFvxEx:
sub rsp, 40
call qword ptr [rcx]
add rsp, 40
ret

.global _V19create_sum_functionv_rPFxxxE
_V19create_sum_functionv_rPFxxxE:
sub rsp, 40
mov rcx, 8
call _V8allocatex_rPh
lea rcx, [rip+_V19create_sum_functionv_rPFxxxE_0_xx_rx]
mov qword ptr [rax], rcx
add rsp, 40
ret

.global _V20execute_sum_functionPFxxxExx_rx
_V20execute_sum_functionPFxxxExx_rx:
sub rsp, 40
call qword ptr [rcx]
add rsp, 40
ret

.global _V25create_capturing_functioncsixd_rPFdE
_V25create_capturing_functioncsixd_rPFdE:
push rbx
push rsi
push rdi
push rbp
sub rsp, 40
mov rbx, rcx
mov rcx, 31
mov rsi, rdx
movsd qword ptr [rsp+112], xmm0
mov rdi, r8
mov rbp, r9
call _V8allocatex_rPh
lea rcx, [rip+_V25create_capturing_functioncsixd_rPFdE_0__rd]
mov qword ptr [rax], rcx
mov byte ptr [rax+8], bl
mov word ptr [rax+9], si
mov dword ptr [rax+11], edi
mov qword ptr [rax+15], rbp
movsd xmm0, qword ptr [rsp+112]
movsd qword ptr [rax+23], xmm0
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

.global _V26execute_capturing_functionPFdE_rd
_V26execute_capturing_functionPFdE_rd:
sub rsp, 40
call qword ptr [rcx]
add rsp, 40
ret

.global _V40create_capturing_function_with_parameterP3DogP3Cat_rPFvdE
_V40create_capturing_function_with_parameterP3DogP3Cat_rPFvdE:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rcx, 24
mov rsi, rdx
call _V8allocatex_rPh
lea rcx, [rip+_V40create_capturing_function_with_parameterP3DogP3Cat_rPFvdE_0_d]
mov qword ptr [rax], rcx
mov qword ptr [rax+8], rbx
mov qword ptr [rax+16], rsi
add rsp, 40
pop rsi
pop rbx
ret

.global _V41execute_capturing_function_with_parameterPFvdEd
_V41execute_capturing_function_with_parameterPFvdEd:
sub rsp, 40
call qword ptr [rcx]
add rsp, 40
ret

.global _V4initv_rx
_V4initv_rx:
push rbx
push rsi
sub rsp, 40
call _VN3Dog4initEv_rPS_
mov rbx, rax
call _VN3Cat4initEv_rPS_
mov rcx, 10
mov rdx, 10
mov rsi, rax
call _VN6Vector4initExx_rPS_
mov qword ptr [rbx+24], rax
xor rcx, rcx
xor rdx, rdx
call _VN6Vector4initExx_rPS_
mov qword ptr [rsi+24], rax
mov rcx, rbx
mov rdx, rsi
call _VN6Animal8interactEPS_
mov rcx, rsi
mov rdx, rbx
call _VN6Animal8interactEPS_
movsd xmm0, qword ptr [rip+_V4initv_rx_C0]
movsd xmm1, qword ptr [rip+_V4initv_rx_C0]
call _VN6Vector4initEdd_rPS_
mov qword ptr [rbx+24], rax
xor rcx, rcx
xor rdx, rdx
call _VN6Vector4initExx_rPS_
mov qword ptr [rsi+24], rax
mov rcx, rbx
mov rdx, rsi
call _VN6Animal8interactEPS_
mov rcx, rsi
mov rdx, rbx
call _VN6Animal8interactEPS_
call _V21create_default_actionv_rPFvE
mov rcx, rax
call _V22execute_default_actionPFvE
call _V20create_number_actionv_rPFvxE
mov rcx, rax
mov rdx, -1
call _V21execute_number_actionPFvxEx
call _V19create_sum_functionv_rPFxxxE
mov rcx, rax
mov rdx, 1
mov r8, 2
call _V20execute_sum_functionPFxxxExx_rx
mov rcx, rax
call _V7printlnx
movsd xmm0, qword ptr [rip+_V4initv_rx_C1]
mov rcx, 1
mov rdx, 2
mov r8, 3
mov r9, 4
call _V25create_capturing_functioncsixd_rPFdE
mov rcx, rax
call _V26execute_capturing_functionPFdE_rd
call _V7printlnd
mov rcx, rbx
mov rdx, rsi
call _V40create_capturing_function_with_parameterP3DogP3Cat_rPFvdE
mov rcx, rax
movsd xmm0, qword ptr [rip+_V4initv_rx_C2]
mov rbx, rax
call _V41execute_capturing_function_with_parameterPFvdEd
mov rcx, rbx
movsd xmm0, qword ptr [rip+_V4initv_rx_C3]
call _V41execute_capturing_function_with_parameterPFvdEd
xor rax, rax
add rsp, 40
pop rsi
pop rbx
ret

.global _VN3Dog4initEv_rPS__0_P6Animal_rP6Vector
_VN3Dog4initEv_rPS__0_P6Animal_rP6Vector:
push rbx
push rsi
push rdi
sub rsp, 64
mov r9, [rcx+8]
mov r8, [r9+24]
movsd xmm0, qword ptr [r8+8]
mov r10, [rdx+24]
subsd xmm0, qword ptr [r10+8]
movsd xmm1, qword ptr [rip+_VN3Dog4initEv_rPS__0_P6Animal_rP6Vector_C0]
mov rbx, rcx
mov rsi, rdx
call _V3powdd_rd
mov rdx, [rbx+8]
mov rcx, [rdx+24]
movsd xmm1, qword ptr [rcx+16]
mov r8, [rsi+24]
subsd xmm1, qword ptr [r8+16]
movsd qword ptr [rsp+48], xmm0
movsd xmm0, xmm1
movsd xmm1, qword ptr [rip+_VN3Dog4initEv_rPS__0_P6Animal_rP6Vector_C0]
call _V3powdd_rd
movsd xmm1, qword ptr [rsp+48]
addsd xmm1, xmm0
movsd xmm0, xmm1
call _V4sqrtd_rd
mov rcx, [rsi+24]
mov r8, [rbx+8]
mov rdx, [r8+24]
movsd qword ptr [rsp+56], xmm0
call _VN6Vector5minusEPS__rS0_
mov rdi, rax
movsd xmm0, qword ptr [rsp+56]
movsd xmm1, qword ptr [rip+_VN3Dog4initEv_rPS__0_P6Animal_rP6Vector_C1]
comisd xmm0, xmm1
ja _VN3Dog4initEv_rPS__0_P6Animal_rP6Vector_L1
mov rcx, [rsi+16]
cmp rcx, 1
jne _VN3Dog4initEv_rPS__0_P6Animal_rP6Vector_L1
mov rcx, rdi
call _VN6Vector6invertEv
mov rcx, rdi
mov rdx, 10
call _VN6Vector12assign_timesEx
mov rcx, [rbx+8]
call _VN3Dog4barkEv
mov rcx, [rbx+8]
call _VN3Dog4barkEv
jmp _VN3Dog4initEv_rPS__0_P6Animal_rP6Vector_L0
_VN3Dog4initEv_rPS__0_P6Animal_rP6Vector_L1:
mov rcx, [rbx+8]
call _VN3Dog4barkEv
_VN3Dog4initEv_rPS__0_P6Animal_rP6Vector_L0:
mov rax, rdi
add rsp, 64
pop rdi
pop rsi
pop rbx
ret

.global _VN3Cat4initEv_rPS__0_P6Animal_rP6Vector
_VN3Cat4initEv_rPS__0_P6Animal_rP6Vector:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rcx, [rbx+8]
mov rsi, rdx
call _VN3Cat4meowEv
mov rcx, [rsi+24]
mov r8, [rbx+8]
mov rdx, [r8+24]
call _VN6Vector5minusEPS__rS0_
mov rcx, rax
mov rdx, 2
call _VN6Vector5timesEx_rPS_
add rsp, 40
pop rsi
pop rbx
ret

.global _V21create_default_actionv_rPFvE_0_
_V21create_default_actionv_rPFvE_0_:
sub rsp, 40
lea rcx, [rip+_V21create_default_actionv_rPFvE_0__S0]
call _V7printlnPh
add rsp, 40
ret

.global _V20create_number_actionv_rPFvxE_0_x
_V20create_number_actionv_rPFvxE_0_x:
sub rsp, 40
mov rcx, rdx
call _V9to_stringx_rP6String
mov rcx, rax
call _V7printlnP6String
add rsp, 40
ret

.global _V19create_sum_functionv_rPFxxxE_0_xx_rx
_V19create_sum_functionv_rPFxxxE_0_xx_rx:
lea rax, [rdx+r8]
ret

.global _V25create_capturing_functioncsixd_rPFdE_0__rd
_V25create_capturing_functioncsixd_rPFdE_0__rd:
movsx rdx, byte ptr [rcx+8]
movsx r8, word ptr [rcx+9]
add rdx, r8
movsxd r8, dword ptr [rcx+11]
add rdx, r8
add rdx, [rcx+15]
cvtsi2sd xmm0, rdx
addsd xmm0, qword ptr [rcx+23]
ret

.global _V40create_capturing_function_with_parameterP3DogP3Cat_rPFvdE_0_d
_V40create_capturing_function_with_parameterP3DogP3Cat_rPFvdE_0_d:
push rbx
sub rsp, 32
mov rbx, rcx
mov rcx, 1
mov rdx, 1
movsd qword ptr [rsp+56], xmm0
call _VN6Vector4initExx_rPS_
movsd xmm0, qword ptr [rsp+56]
movsd xmm1, xmm0
movsd xmm2, qword ptr [rip+_V40create_capturing_function_with_parameterP3DogP3Cat_rPFvdE_0_d_C0]
mulsd xmm1, xmm2
mov rcx, rax
movsd qword ptr [rsp+56], xmm0
movsd xmm0, xmm1
call _VN6Vector5timesEd_rPS_
mov rcx, [rbx+8]
mov qword ptr [rcx+24], rax
mov rcx, 1
mov rdx, 1
call _VN6Vector4initExx_rPS_
movsd xmm0, qword ptr [rsp+56]
movsd xmm1, qword ptr [rip+_V40create_capturing_function_with_parameterP3DogP3Cat_rPFvdE_0_d_C0]
mulsd xmm0, xmm1
xorpd xmm0, oword ptr [rip+_V40create_capturing_function_with_parameterP3DogP3Cat_rPFvdE_0_d_C1]
mov rcx, rax
call _VN6Vector5timesEd_rPS_
mov rcx, [rbx+16]
mov qword ptr [rcx+24], rax
mov rcx, [rbx+8]
mov rdx, [rbx+16]
call _VN6Animal8interactEPS_
mov rcx, [rbx+16]
mov rdx, [rbx+8]
call _VN6Animal8interactEPS_
add rsp, 32
pop rbx
ret

.section .data

_VN6Vector_configuration:
.quad _VN6Vector_descriptor

_VN6Vector_descriptor:
.quad _VN6Vector_descriptor_0
.long 24
.long 0

_VN6Vector_descriptor_0:
.ascii "Vector"
.byte 0
.byte 1
.byte 2
.byte 0

_VN6Animal_configuration:
.quad _VN6Animal_descriptor

_VN6Animal_descriptor:
.quad _VN6Animal_descriptor_0
.long 32
.long 0

_VN6Animal_descriptor_0:
.ascii "Animal"
.byte 0
.byte 1
.byte 2
.byte 0

_VN3Dog_configuration:
.quad _VN3Dog_descriptor
.quad _VN3Dog_descriptor

_VN3Dog_descriptor:
.quad _VN3Dog_descriptor_0
.long 40
.long 1
.quad _VN6Animal_descriptor

_VN3Dog_descriptor_0:
.ascii "Dog"
.byte 0
.byte 1
.ascii "Animal"
.byte 1
.byte 2
.byte 0

_VN3Cat_configuration:
.quad _VN3Cat_descriptor
.quad _VN3Cat_descriptor

_VN3Cat_descriptor:
.quad _VN3Cat_descriptor_0
.long 40
.long 1
.quad _VN6Animal_descriptor

_VN3Cat_descriptor_0:
.ascii "Cat"
.byte 0
.byte 1
.ascii "Animal"
.byte 1
.byte 2
.byte 0

.balign 16
_VN3Dog4barkEv_S0:
.ascii "Bark"
.byte 0
.balign 16
_VN3Cat4meowEv_S0:
.ascii "Meow"
.byte 0
.balign 16
_V21create_default_actionv_rPFvE_0__S0:
.ascii "Hi there!"
.byte 0

.balign 16
_VN6Vector6invertEv_C0:
.byte 0, 0, 0, 0, 0, 0, 0, 128, 0, 0, 0, 0, 0, 0, 0, 128
.balign 16
_VN6Vector6invertEv_C1:
.byte 0, 0, 0, 0, 0, 0, 0, 128, 0, 0, 0, 0, 0, 0, 0, 128
.balign 16
_V4initv_rx_C0:
.byte 0, 0, 0, 0, 0, 0, 224, 63 # 0.5
.balign 16
_V4initv_rx_C1:
.byte 0, 0, 0, 0, 0, 0, 20, 64 # 5.0
.balign 16
_V4initv_rx_C2:
.byte 57, 180, 200, 118, 190, 159, 246, 63 # 1.414
.balign 16
_V4initv_rx_C3:
.byte 154, 153, 153, 153, 153, 153, 185, 191 # -0.1
.balign 16
_VN3Dog4initEv_rPS__0_P6Animal_rP6Vector_C0:
.byte 0, 0, 0, 0, 0, 0, 0, 64 # 2.0
.balign 16
_VN3Dog4initEv_rPS__0_P6Animal_rP6Vector_C1:
.byte 0, 0, 0, 0, 0, 0, 240, 63 # 1.0
.balign 16
_V40create_capturing_function_with_parameterP3DogP3Cat_rPFvdE_0_d_C0:
.byte 0, 0, 0, 0, 0, 0, 224, 63 # 0.5
.balign 16
_V40create_capturing_function_with_parameterP3DogP3Cat_rPFvdE_0_d_C1:
.byte 0, 0, 0, 0, 0, 0, 0, 128, 0, 0, 0, 0, 0, 0, 0, 128

