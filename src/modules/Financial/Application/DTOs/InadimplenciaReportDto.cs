using Giglio.EduCore.Financial.Domain.Enums;

namespace Giglio.EduCore.Financial.Application.DTOs;

/// <summary>Relatório completo de inadimplência por turma.</summary>
public sealed record InadimplenciaReportDto(
    TurmaInadimplenciaDto Turma,
    int TotalAlunos,
    int AlunosInadimplentes,
    decimal PercentualInadimplencia,
    decimal ValorTotalDevido,
    decimal ValorTotalRecebido,
    IReadOnlyList<AlunoInadimplenciaDto> Alunos,
    int TotalPages,
    int CurrentPage);

/// <summary>Dados resumidos da turma.</summary>
public sealed record TurmaInadimplenciaDto(
    Guid Id,
    string Nome,
    string Turno,
    int AnoLetivo);

/// <summary>Dados de um aluno no relatório de inadimplência.</summary>
public sealed record AlunoInadimplenciaDto(
    Guid AlunoId,
    string Nome,
    string Documento,
    string? PlanoNome,
    decimal ValorMensalidade,
    string StatusGeral,
    int MesesEmAtraso,
    decimal ValorDevido,
    decimal ValorPago,
    IReadOnlyList<ChargeDetailDto> Charges);

/// <summary>Detalhe de uma mensalidade/cobrança individual.</summary>
public sealed record ChargeDetailDto(
    Guid ChargeId,
    int Mes,
    int Ano,
    decimal Valor,
    DateTime Vencimento,
    string Status,
    DateTime? DataPagamento,
    decimal? ValorPago);
