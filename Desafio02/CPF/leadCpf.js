function mascaraCPF(executionContext) {
    let formContext = executionContext.getFormContext();
    let campo = formContext.getAttribute("new_cpf").getValue();

    if (campo) {
        let valor = campo.replace(/\D/g, '');
        valor = valor.replace(/(\d{3})(\d)/, '$1.$2');
        valor = valor.replace(/(\d{3})(\d)/, '$1.$2');
        valor = valor.replace(/(\d{3})(\d{1,2})$/, '$1-$2');
        formContext.getAttribute("new_cpf").setValue(valor);
    }
}


function validaCPF(executionContext) {
    var formContext = executionContext.getFormContext();
    var campo = formContext.getAttribute("new_cpf").getValue();

    if (campo == null) {
        return;
    }

    let valorCpf = campo.replace(/\D/g, '');

    if (valorCpf.length !== 11 || /^(\d)\1{10}$/.test(valorCpf)) {
        formContext.getControl("new_cpf").setNotification("CPF inválido, favor corrigir!"); 
        return;
    }

    const calcularDigito = (cpf, peso) => {
        let soma = 0;
        for (let i = 0; i < peso; i++) {
            soma += cpf[i] * (peso + 1 - i);
        }
        const resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    };

    const arrayCpf = valorCpf.split('');

    const digito1 = calcularDigito(arrayCpf, 9);
    const digito2 = calcularDigito(arrayCpf, 10);

    if (valorCpf.charAt(9) != digito1 || valorCpf.charAt(10) != digito2) {
        formContext.getControl("new_cpf").setNotification("CPF inválido, favor corrigir!"); 
        return;
    }

    formContext.getControl("new_cpf").clearNotification(); 
}