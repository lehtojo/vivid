.section .text
.intel_syntax noprefix
.global main
main:
jmp _V4initv_rx

.extern _V4copyPhxS_
.extern _V3powdd_rd
.extern _V4sqrtd_rd
.extern _V14internal_printPhx
.extern _V4exitx
.extern _V17internal_allocatex_rPh

.global _VN7Vehicle4timeEd_rd
_VN7Vehicle4timeEd_rd:
sub rsp, 40
movsd xmm1, qword ptr [rip+_VN7Vehicle4timeEd_rd_C0]
mulsd xmm1, xmm0
divsd xmm1, qword ptr [rcx+32]
movsd xmm0, xmm1
call _V4sqrtd_rd
add rsp, 40
ret

.global _VN3Pig5skillEv_rx
_VN3Pig5skillEv_rx_v:
sub rcx, 8
_VN3Pig5skillEv_rx:
mov rax, 1
ret

.global _VN3Pig11reliabilityEv_rx
_VN3Pig11reliabilityEv_rx_v:
sub rcx, 8
_VN3Pig11reliabilityEv_rx:
mov rax, -1
ret

.global _VN3Pig5likesEP6Entity_rb
_VN3Pig5likesEP6Entity_rb_v:
sub rcx, 8
_VN3Pig5likesEP6Entity_rb:
push rbx
push rdi
push rsi
push rbp
push r12
sub rsp, 32
xor rsi, rsi
mov rbp, rdx
xor r12, r12
mov rdi, rbp
mov rcx, [rbp]
mov rbx, [rcx]
lea rcx, [rip+_VN6Person_descriptor]
cmp rbx, rcx
je _VN3Pig5likesEP6Entity_rb_L4
mov rcx, rbx
lea rdx, [rip+_VN6Person_descriptor]
call _V8inheritsPhS__rx
test rax, rax
je _VN3Pig5likesEP6Entity_rb_L3
_VN3Pig5likesEP6Entity_rb_L4:
mov rsi, rdi
_VN3Pig5likesEP6Entity_rb_L3:
test rsi, rsi
jz _VN3Pig5likesEP6Entity_rb_L0
movzx rcx, byte ptr [rsi+33]
test rcx, rcx
jz _VN3Pig5likesEP6Entity_rb_L0
mov r12, 1
_VN3Pig5likesEP6Entity_rb_L0:
mov rax, r12
add rsp, 32
pop r12
pop rbp
pop rsi
pop rdi
pop rbx
ret

.global _VN3Car5skillEv_rx
_VN3Car5skillEv_rx_v:
sub rcx, 8
_VN3Car5skillEv_rx:
mov rax, 10
ret

.global _VN3Car11reliabilityEv_rx
_VN3Car11reliabilityEv_rx_v:
sub rcx, 8
_VN3Car11reliabilityEv_rx:
mov rax, 100
ret

.global _VN3Car5likesEP6Entity_rb
_VN3Car5likesEP6Entity_rb_v:
sub rcx, 8
_VN3Car5likesEP6Entity_rb:
push rbx
push rdi
push rsi
push rbp
push r12
sub rsp, 32
xor rsi, rsi
mov rbp, rdx
xor r12, r12
mov rdi, rbp
mov rcx, [rbp]
mov rbx, [rcx]
lea rcx, [rip+_VN6Person_descriptor]
cmp rbx, rcx
je _VN3Car5likesEP6Entity_rb_L4
mov rcx, rbx
lea rdx, [rip+_VN6Person_descriptor]
call _V8inheritsPhS__rx
test rax, rax
je _VN3Car5likesEP6Entity_rb_L3
_VN3Car5likesEP6Entity_rb_L4:
mov rsi, rdi
_VN3Car5likesEP6Entity_rb_L3:
test rsi, rsi
jz _VN3Car5likesEP6Entity_rb_L0
movzx rcx, byte ptr [rsi+32]
test rcx, rcx
jz _VN3Car5likesEP6Entity_rb_L0
mov r12, 1
_VN3Car5likesEP6Entity_rb_L0:
mov rax, r12
add rsp, 32
pop r12
pop rbp
pop rsi
pop rdi
pop rbx
ret

.global _VN6Banana5likesEP6Entity_rb
_VN6Banana5likesEP6Entity_rb_v:
_VN6Banana5likesEP6Entity_rb:
mov rax, 1
ret

.global _VN3Bus5skillEv_rx
_VN3Bus5skillEv_rx_v:
sub rcx, 8
_VN3Bus5skillEv_rx:
mov rax, 40
ret

.global _VN3Bus11reliabilityEv_rx
_VN3Bus11reliabilityEv_rx_v:
sub rcx, 8
_VN3Bus11reliabilityEv_rx:
mov rax, 100
ret

.global _VN3Bus5likesEP6Entity_rb
_VN3Bus5likesEP6Entity_rb_v:
sub rcx, 8
_VN3Bus5likesEP6Entity_rb:
push rbx
push rdi
push rsi
push rbp
push r12
sub rsp, 32
xor rsi, rsi
mov rbp, rdx
xor r12, r12
mov rdi, rbp
mov rcx, [rbp]
mov rbx, [rcx]
lea rcx, [rip+_VN6Person_descriptor]
cmp rbx, rcx
je _VN3Bus5likesEP6Entity_rb_L4
mov rcx, rbx
lea rdx, [rip+_VN6Person_descriptor]
call _V8inheritsPhS__rx
test rax, rax
je _VN3Bus5likesEP6Entity_rb_L3
_VN3Bus5likesEP6Entity_rb_L4:
mov rsi, rdi
_VN3Bus5likesEP6Entity_rb_L3:
test rsi, rsi
jz _VN3Bus5likesEP6Entity_rb_L0
movzx rcx, byte ptr [rsi+32]
test rcx, rcx
jz _VN3Bus5likesEP6Entity_rb_L0
mov r12, 1
_VN3Bus5likesEP6Entity_rb_L0:
mov rax, r12
add rsp, 32
pop r12
pop rbp
pop rsi
pop rdi
pop rbx
ret

.global _VN6Person4initEPhxbb_rPS_
_VN6Person4initEPhxbb_rPS_:
push rbx
push rsi
push rdi
push rbp
sub rsp, 40
mov rbx, rcx
mov rcx, 34
mov rsi, rdx
mov rdi, r8
mov rbp, r9
call _V8allocatex_rPh
lea rcx, [rip+_VN6Person_configuration+8]
mov qword ptr [rax], rcx
mov rcx, rbx
mov rbx, rax
call _VN6String4initEPh_rPS_
mov qword ptr [rbx+16], rax
mov qword ptr [rbx+24], rsi
mov byte ptr [rbx+32], dil
mov byte ptr [rbx+33], bpl
mov rax, rbx
add rsp, 40
pop rbp
pop rdi
pop rsi
pop rbx
ret

.global _VN3Pig4initEv_rPS_
_VN3Pig4initEv_rPS_:
sub rsp, 40
mov rcx, 58
call _V8allocatex_rPh
lea rcx, [rip+_VN3Pig_configuration+48]
mov qword ptr [rax+8], rcx
lea rcx, [rip+_VN3Pig_configuration+16]
mov qword ptr [rax+16], rcx
lea rcx, [rip+_VN3Pig_configuration+8]
mov qword ptr [rax], rcx
mov rcx, 4619567317775286272
mov qword ptr [rax+24], rcx
mov qword ptr [rax+32], 100
mov rcx, 4613937818241073152
mov qword ptr [rax+40], rcx
mov word ptr [rax+48], 1
add rsp, 40
ret

.global _VN3Car4initEv_rPS_
_VN3Car4initEv_rPS_:
sub rsp, 40
mov rcx, 58
call _V8allocatex_rPh
lea rcx, [rip+_VN3Car_configuration+48]
mov qword ptr [rax+8], rcx
lea rcx, [rip+_VN3Car_configuration+16]
mov qword ptr [rax+16], rcx
lea rcx, [rip+_VN3Car_configuration+8]
mov qword ptr [rax], rcx
mov rcx, 4632937379169042432
mov qword ptr [rax+24], rcx
mov qword ptr [rax+32], 1500
movsd xmm0, qword ptr [rip+_VN3Car4initEv_rPS__C0]
movsd qword ptr [rax+40], xmm0
mov word ptr [rax+48], 5
add rsp, 40
ret

.global _VN6Banana4initEv_rPS_
_VN6Banana4initEv_rPS_:
sub rsp, 40
mov rcx, 24
call _V8allocatex_rPh
lea rcx, [rip+_VN6Banana_configuration+24]
mov qword ptr [rax+8], rcx
lea rcx, [rip+_VN6Banana_configuration+8]
mov qword ptr [rax], rcx
add rsp, 40
ret

.global _VN3Bus4initEv_rPS_
_VN3Bus4initEv_rPS_:
sub rsp, 40
mov rcx, 58
call _V8allocatex_rPh
lea rcx, [rip+_VN3Bus_configuration+48]
mov qword ptr [rax+8], rcx
lea rcx, [rip+_VN3Bus_configuration+16]
mov qword ptr [rax+16], rcx
lea rcx, [rip+_VN3Bus_configuration+8]
mov qword ptr [rax], rcx
mov rcx, 4630826316843712512
mov qword ptr [rax+24], rcx
mov qword ptr [rax+32], 4000
movsd xmm0, qword ptr [rip+_VN3Bus4initEv_rPS__C0]
movsd qword ptr [rax+40], xmm0
mov word ptr [rax+48], 40
add rsp, 40
ret

.global _V7can_useP6EntityP6Usable_rb
_V7can_useP6EntityP6Usable_rb:
push rbx
push rsi
push rdi
push rbp
push r12
push r13
push r14
push r15
sub rsp, 40
mov r12, rdx
xor r13, r13
mov r14, rcx
xor r15, r15
mov rcx, r12
mov rdx, r14
mov r8, [r12]
call qword ptr [r8+8]
movzx rax, al
xor rax, 1
test rax, rax
jz _V7can_useP6EntityP6Usable_rb_L1
xor rax, rax
add rsp, 40
pop r15
pop r14
pop r13
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret
jmp _V7can_useP6EntityP6Usable_rb_L0
_V7can_useP6EntityP6Usable_rb_L1:
mov rbp, r12
mov rcx, [r12]
mov rsi, [rcx]
lea rcx, [rip+_VN7Vehicle_descriptor]
cmp rsi, rcx
je _V7can_useP6EntityP6Usable_rb_L6
mov rcx, rsi
lea rdx, [rip+_VN7Vehicle_descriptor]
call _V8inheritsPhS__rx
test rax, rax
je _V7can_useP6EntityP6Usable_rb_L5
_V7can_useP6EntityP6Usable_rb_L6:
mov r13, rbp
_V7can_useP6EntityP6Usable_rb_L5:
test r13, r13
jz _V7can_useP6EntityP6Usable_rb_L0
mov rdi, r14
mov rcx, [r14]
mov rbx, [rcx]
lea rcx, [rip+_VN6Person_descriptor]
cmp rbx, rcx
je _V7can_useP6EntityP6Usable_rb_L9
mov rcx, rbx
lea rdx, [rip+_VN6Person_descriptor]
call _V8inheritsPhS__rx
test rax, rax
je _V7can_useP6EntityP6Usable_rb_L8
_V7can_useP6EntityP6Usable_rb_L9:
mov r15, rdi
_V7can_useP6EntityP6Usable_rb_L8:
test r15, r15
jz _V7can_useP6EntityP6Usable_rb_L0
xor rbx, rbx
mov rsi, [r15+24]
mov rcx, r13
mov rdx, [r13+8]
call qword ptr [rdx+8]
cmp rsi, rax
jl _V7can_useP6EntityP6Usable_rb_L11
mov rbx, 1
_V7can_useP6EntityP6Usable_rb_L11:
mov rax, rbx
add rsp, 40
pop r15
pop r14
pop r13
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret
_V7can_useP6EntityP6Usable_rb_L0:
xor rax, rax
add rsp, 40
pop r15
pop r14
pop r13
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

.global _V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE
_V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE:
push rbx
push rsi
push rdi
push rbp
push r12
sub rsp, 32
mov rbx, rcx
mov rsi, rdx
call _VN4ListIP7VehicleE4initEv_rS1_
xor rdi, rdi
mov rbp, rax
cmp rdi, [rbx+16]
jge _V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L1
_V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L0:
mov rcx, rbx
mov rdx, rdi
call _VN5ArrayIP6UsableE3getEx_rS0_
mov rdx, [rax]
mov rcx, [rdx]
lea r8, [rip+_VN7Vehicle_descriptor]
cmp rcx, r8
je _V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L4
mov rcx, rbx
mov rdx, rdi
call _VN5ArrayIP6UsableE3getEx_rS0_
mov rdx, [rax]
mov rcx, [rdx]
lea rdx, [rip+_VN7Vehicle_descriptor]
call _V8inheritsPhS__rx
test rax, rax
je _V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L3
_V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L4:
mov rcx, rbx
mov rdx, rdi
call _VN5ArrayIP6UsableE3getEx_rS0_
mov rcx, rbp
mov rdx, rax
call _VN4ListIP7VehicleE3addES0_
_V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L3:
add rdi, 1
cmp rdi, [rbx+16]
jl _V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L0
_V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L1:
mov rcx, rbp
call _VN4ListIP7VehicleE4sizeEv_rx
sub rax, 1
mov rdi, rax
test rdi, rdi
jl _V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L8
_V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L7:
mov rcx, rbp
mov rdx, rdi
call _VN4ListIP7VehicleE3getEx_rS0_
mov rcx, rbp
mov rdx, rdi
mov r12, rax
call _VN4ListIP7VehicleE3getEx_rS0_
mov rcx, r12
mov rdx, [rax+8]
call qword ptr [rdx+16]
cmp rax, rsi
jge _V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L10
mov rcx, rbp
mov rdx, rdi
call _VN4ListIP7VehicleE6removeEx
_V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L10:
sub rdi, 1
test rdi, rdi
jge _V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L7
_V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE_L8:
mov rax, rbp
add rsp, 32
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

.global _V14choose_vehicleP6EntityP4ListIP7VehicleEx_rS2_
_V14choose_vehicleP6EntityP4ListIP7VehicleEx_rS2_:
sub rsp, 40
cvtsi2sd xmm0, r8
call _V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2_
add rsp, 40
ret

.global _V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2_
_V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2_:
push rbx
push rsi
push rdi
push rbp
push r12
push r13
push r14
sub rsp, 48
xor rdi, rdi
mov rbp, rcx
mov rsi, rbp
mov rcx, [rbp]
mov rbx, [rcx]
lea rcx, [rip+_VN6Person_descriptor]
cmp rbx, rcx
je _V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L4
mov rcx, rbx
mov r12, rdx
lea rdx, [rip+_VN6Person_descriptor]
movsd qword ptr [rsp+128], xmm0
call _V8inheritsPhS__rx
test rax, rax
mov rdx, r12
movsd xmm0, qword ptr [rsp+128]
je _V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L3
_V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L4:
mov rdi, rsi
_V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L3:
test rdi, rdi
jz _V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L0
mov rcx, [rdi+16]
mov rbx, rdx
lea rdx, [rip+_V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__S0]
movsd qword ptr [rsp+128], xmm0
call _VN6String6equalsEPh_rb
movzx rax, al
test rax, rax
mov rdx, rbx
movsd xmm0, qword ptr [rsp+128]
jz _V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L0
mov rbx, rdx
movsd qword ptr [rsp+128], xmm0
call _VN3Pig4initEv_rPS_
lea rax, [rax+8]
add rsp, 48
pop r14
pop r13
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret
mov rdx, rbx
movsd xmm0, qword ptr [rsp+24]
_V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L0:
mov rcx, rdx
mov r12, rdx
xor rdx, rdx
movsd qword ptr [rsp+128], xmm0
call _VN4ListIP7VehicleE3getEx_rS0_
mov rcx, r12
xor rdx, rdx
mov r13, rax
call _VN4ListIP7VehicleE3getEx_rS0_
mov rcx, rax
movsd xmm0, qword ptr [rsp+128]
call _VN7Vehicle4timeEd_rd
mov r14, 1
mov rcx, r12
movsd qword ptr [rsp+40], xmm0
call _VN4ListIP7VehicleE4sizeEv_rx
cmp r14, rax
movsd xmm0, qword ptr [rsp+40]
jge _V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L7
_V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L6:
mov rcx, r12
mov rdx, r14
movsd qword ptr [rsp+40], xmm0
call _VN4ListIP7VehicleE3getEx_rS0_
mov rcx, rax
movsd xmm0, qword ptr [rsp+128]
mov rbx, rax
call _VN7Vehicle4timeEd_rd
movsd xmm1, qword ptr [rsp+40]
comisd xmm0, xmm1
jae _V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L9
mov r13, rbx
movsd xmm1, xmm0
_V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L9:
add r14, 1
movsd qword ptr [rsp+40], xmm1
mov rcx, r12
call _VN4ListIP7VehicleE4sizeEv_rx
cmp r14, rax
movsd xmm0, qword ptr [rsp+40]
jl _V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L6
_V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__L7:
mov rax, r13
add rsp, 48
pop r14
pop r13
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

.global _V10create_pigv_rP3Pig
_V10create_pigv_rP3Pig:
sub rsp, 40
call _VN3Pig4initEv_rPS_
add rsp, 40
ret

.global _V10create_busv_rP3Bus
_V10create_busv_rP3Bus:
sub rsp, 40
call _VN3Bus4initEv_rPS_
add rsp, 40
ret

.global _V10create_carv_rP3Car
_V10create_carv_rP3Car:
sub rsp, 40
call _VN3Car4initEv_rPS_
add rsp, 40
ret

.global _V13create_bananav_rP6Banana
_V13create_bananav_rP6Banana:
sub rsp, 40
call _VN6Banana4initEv_rPS_
add rsp, 40
ret

.global _V11create_johnv_rP6Person
_V11create_johnv_rP6Person:
sub rsp, 40
lea rcx, [rip+_V11create_johnv_rP6Person_S0]
mov rdx, 10
mov r8, 1
xor r9, r9
call _VN6Person4initEPhxbb_rPS_
add rsp, 40
ret

.global _V10create_maxv_rP6Person
_V10create_maxv_rP6Person:
sub rsp, 40
lea rcx, [rip+_V10create_maxv_rP6Person_S0]
mov rdx, 7
mov r8, 1
mov r9, 1
call _VN6Person4initEPhxbb_rPS_
add rsp, 40
ret

.global _V11create_gabev_rP6Person
_V11create_gabev_rP6Person:
sub rsp, 40
lea rcx, [rip+_V11create_gabev_rP6Person_S0]
mov rdx, 50
mov r8, 1
xor r9, r9
call _VN6Person4initEPhxbb_rPS_
add rsp, 40
ret

.global _V12create_stevev_rP6Person
_V12create_stevev_rP6Person:
sub rsp, 40
lea rcx, [rip+_V12create_stevev_rP6Person_S0]
mov rdx, 1
xor r8, r8
mov r9, 1
call _VN6Person4initEPhxbb_rPS_
add rsp, 40
ret

.global _V12create_arrayx_rP5ArrayIP6UsableE
_V12create_arrayx_rP5ArrayIP6UsableE:
sub rsp, 40
call _VN5ArrayIP6UsableE4initEx_rS1_
add rsp, 40
ret

.global _V3setP5ArrayIP6UsableES0_x
_V3setP5ArrayIP6UsableES0_x:
sub rsp, 40
xchg r8, rdx
call _VN5ArrayIP6UsableE3setExS0_
add rsp, 40
ret

.global _V6is_pigP7Vehicle_rb
_V6is_pigP7Vehicle_rb:
push rbx
push rsi
push rdi
sub rsp, 32
mov rsi, rcx
xor rdi, rdi
mov rcx, [rsi]
mov rbx, [rcx]
lea rcx, [rip+_VN3Pig_descriptor]
cmp rbx, rcx
je _V6is_pigP7Vehicle_rb_L1
mov rcx, rbx
lea rdx, [rip+_VN3Pig_descriptor]
call _V8inheritsPhS__rx
test rax, rax
je _V6is_pigP7Vehicle_rb_L0
_V6is_pigP7Vehicle_rb_L1:
mov rdi, 1
_V6is_pigP7Vehicle_rb_L0:
mov rax, rdi
add rsp, 32
pop rdi
pop rsi
pop rbx
ret

.global _V4initv_rx
_V4initv_rx:
push rbx
push rsi
push rdi
push rbp
push r12
sub rsp, 32
call _V11create_johnv_rP6Person
mov rbx, rax
call _V10create_pigv_rP3Pig
mov rsi, rax
call _V10create_busv_rP3Bus
mov rdi, rax
call _V10create_carv_rP3Car
mov rbp, rax
call _V13create_bananav_rP6Banana
mov rcx, 4
mov r12, rax
call _V12create_arrayx_rP5ArrayIP6UsableE
mov rcx, rax
xor rdx, rdx
lea r8, [rsi+8]
mov rsi, rax
call _VN5ArrayIP6UsableE3setExS0_
mov rcx, rsi
mov rdx, 1
lea r8, [rdi+8]
call _VN5ArrayIP6UsableE3setExS0_
mov rcx, rsi
mov rdx, 2
lea r8, [rbp+8]
call _VN5ArrayIP6UsableE3setExS0_
mov rcx, rsi
mov rdx, 3
mov r8, r12
call _VN5ArrayIP6UsableE3setExS0_
mov rcx, rsi
mov rdx, 10
call _V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE
mov rcx, rbx
mov rdx, rax
mov r8, 7000
call _V14choose_vehicleP6EntityP4ListIP7VehicleEx_rS2_
mov rax, 1
add rsp, 32
pop r12
pop rbp
pop rdi
pop rsi
pop rbx
ret

.section .data

_VN6Entity_configuration:
.quad _VN6Entity_descriptor

_VN6Entity_descriptor:
.quad _VN6Entity_descriptor_0
.long 8
.long 0

_VN6Entity_descriptor_0:
.ascii "Entity"
.byte 0
.byte 1
.byte 2
.byte 0

_VN6Person_configuration:
.quad _VN6Person_descriptor
.quad _VN6Person_descriptor

_VN6Person_descriptor:
.quad _VN6Person_descriptor_0
.long 34
.long 1
.quad _VN6Entity_descriptor

_VN6Person_descriptor_0:
.ascii "Person"
.byte 0
.byte 1
.ascii "Entity"
.byte 1
.byte 2
.byte 0

_VN6Usable_configuration:
.quad _VN6Usable_descriptor

_VN6Usable_descriptor:
.quad _VN6Usable_descriptor_0
.long 8
.long 0

_VN6Usable_descriptor_0:
.ascii "Usable"
.byte 0
.byte 1
.byte 2
.byte 0

_VN7Vehicle_configuration:
.quad _VN7Vehicle_descriptor
.quad _VN7Vehicle_descriptor

_VN7Vehicle_descriptor:
.quad _VN7Vehicle_descriptor_0
.long 42
.long 1
.quad _VN6Usable_descriptor

_VN7Vehicle_descriptor_0:
.ascii "Vehicle"
.byte 0
.byte 1
.ascii "Usable"
.byte 1
.byte 2
.byte 0

_VN8Drivable_configuration:
.quad _VN8Drivable_descriptor

_VN8Drivable_descriptor:
.quad _VN8Drivable_descriptor_0
.long 8
.long 0

_VN8Drivable_descriptor_0:
.ascii "Drivable"
.byte 0
.byte 1
.byte 2
.byte 0

_VN7Ridable_configuration:
.quad _VN7Ridable_descriptor

_VN7Ridable_descriptor:
.quad _VN7Ridable_descriptor_0
.long 8
.long 0

_VN7Ridable_descriptor_0:
.ascii "Ridable"
.byte 0
.byte 1
.byte 2
.byte 0

_VN3Pig_configuration:
.quad _VN3Pig_descriptor
.quad _VN3Pig_descriptor
.quad _VN3Pig_descriptor
.quad _VN3Pig5skillEv_rx_v
.quad _VN3Pig11reliabilityEv_rx_v
.quad _VN3Pig5likesEP6Entity_rb_v
.quad _VN3Pig_descriptor
.quad _VN3Pig5likesEP6Entity_rb_v

_VN3Pig_descriptor:
.quad _VN3Pig_descriptor_0
.long 58
.long 2
.quad _VN7Ridable_descriptor
.quad _VN7Vehicle_descriptor

_VN3Pig_descriptor_0:
.ascii "Pig"
.byte 0
.byte 1
.ascii "Ridable"
.byte 1
.ascii "Vehicle"
.byte 1
.ascii "Usable"
.byte 1
.byte 2
.byte 0

_VN3Car_configuration:
.quad _VN3Car_descriptor
.quad _VN3Car_descriptor
.quad _VN3Car_descriptor
.quad _VN3Car5skillEv_rx_v
.quad _VN3Car11reliabilityEv_rx_v
.quad _VN3Car5likesEP6Entity_rb_v
.quad _VN3Car_descriptor
.quad _VN3Car5likesEP6Entity_rb_v

_VN3Car_descriptor:
.quad _VN3Car_descriptor_0
.long 58
.long 2
.quad _VN8Drivable_descriptor
.quad _VN7Vehicle_descriptor

_VN3Car_descriptor_0:
.ascii "Car"
.byte 0
.byte 1
.ascii "Drivable"
.byte 1
.ascii "Vehicle"
.byte 1
.ascii "Usable"
.byte 1
.byte 2
.byte 0

_VN6Banana_configuration:
.quad _VN6Banana_descriptor
.quad _VN6Banana_descriptor
.quad _VN6Banana5likesEP6Entity_rb_v
.quad _VN6Banana_descriptor

_VN6Banana_descriptor:
.quad _VN6Banana_descriptor_0
.long 24
.long 2
.quad _VN6Usable_descriptor
.quad _VN6Entity_descriptor

_VN6Banana_descriptor_0:
.ascii "Banana"
.byte 0
.byte 1
.ascii "Usable"
.byte 1
.ascii "Entity"
.byte 1
.byte 2
.byte 0

_VN3Bus_configuration:
.quad _VN3Bus_descriptor
.quad _VN3Bus_descriptor
.quad _VN3Bus_descriptor
.quad _VN3Bus5skillEv_rx_v
.quad _VN3Bus11reliabilityEv_rx_v
.quad _VN3Bus5likesEP6Entity_rb_v
.quad _VN3Bus_descriptor
.quad _VN3Bus5likesEP6Entity_rb_v

_VN3Bus_descriptor:
.quad _VN3Bus_descriptor_0
.long 58
.long 2
.quad _VN8Drivable_descriptor
.quad _VN7Vehicle_descriptor

_VN3Bus_descriptor_0:
.ascii "Bus"
.byte 0
.byte 1
.ascii "Drivable"
.byte 1
.ascii "Vehicle"
.byte 1
.ascii "Usable"
.byte 1
.byte 2
.byte 0

.balign 16
_V14choose_vehicleP6EntityP4ListIP7VehicleEd_rS2__S0:
.ascii "Steve"
.byte 0
.balign 16
_V11create_johnv_rP6Person_S0:
.ascii "John"
.byte 0
.balign 16
_V10create_maxv_rP6Person_S0:
.ascii "Max"
.byte 0
.balign 16
_V11create_gabev_rP6Person_S0:
.ascii "Gabe"
.byte 0
.balign 16
_V12create_stevev_rP6Person_S0:
.ascii "Steve"
.byte 0

.balign 16
_VN7Vehicle4timeEd_rd_C0:
.byte 0, 0, 0, 0, 0, 0, 0, 64 # 2.0
.balign 16
_VN3Car4initEv_rPS__C0:
.byte 184, 30, 133, 235, 81, 56, 22, 64 # 5.555
.balign 16
_VN3Bus4initEv_rPS__C0:
.byte 0, 0, 0, 0, 0, 0, 4, 64 # 2.5

