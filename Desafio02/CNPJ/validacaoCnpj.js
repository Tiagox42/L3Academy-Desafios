function aplicarMascaraCNPJ(executionContext) {
    var formContext = executionContext.getFormContext();  
    var campoCNPJ = formContext.getAttribute("new_cnpj");
    var value = campoCNPJ.getValue();                     
    
    if (value) {
        value = value.replace(/\D/g, "");  
        value = value.replace(/^(\d{2})(\d)/, "$1.$2");  
        value = value.replace(/^(\d{2})\.(\d{3})(\d)/, "$1.$2.$3"); 
        value = value.replace(/\.(\d{3})(\d)/, ".$1/$2");  
        value = value.replace(/(\d{4})(\d)/, "$1-$2");  
        campoCNPJ.setValue(value);

        if (validarCNPJ(value)) {
            formContext.getControl("new_cnpj").clearNotification(); 
        } else {
            formContext.getControl("new_cnpj").setNotification("CNPJ inválido, favor corrigir!"); 
        }
    }
}

function validarCNPJ(cnpj) {
    cnpj = cnpj.replace(/[^\d]+/g, ''); 

    if (cnpj.length !== 14 || /^(\d)\1+$/.test(cnpj)) {
        return false;
    }

    function calcularDigito(cnpj, pesoInicial) {
        let soma = 0;
        let peso = pesoInicial;

        for (let i = 0; i < cnpj.length; i++) {
            soma += cnpj[i] * peso;
            peso--;
            if (peso < 2) peso = 9;
        }

        const resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    const primeiroDigito = calcularDigito(cnpj.substring(0, 12), 5);
    const segundoDigito = calcularDigito(cnpj.substring(0, 12) + primeiroDigito, 6);

    return primeiroDigito == cnpj[12] && segundoDigito == cnpj[13];
}
