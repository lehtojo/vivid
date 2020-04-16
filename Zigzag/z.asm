function_run:
push rbx
mov rbx, 0
cmp rbx, 10
jge function_run_L1
function_run_L0:
push S0
call function_print
add rbx, 1
cmp rbx, 10
jl function_run_L0
function_run_L1:
pop rbx
ret

function_length_of:
mov rax, 0
mov rcx, [rbp]
function_length_of_L0:
mov rdx, [rcx+rax*4]
cmp rdx, 0
jne function_length_of_L1
ret
function_length_of_L1:
add rax, 1
jmp function_length_of_L0
ret

function_print:
push rbx
mov rbx, [rbp]
push rbx
call function_length_of
push rax
push rbx
call function_sys_print
pop rbx
ret