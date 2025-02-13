﻿// <copyright file="DomainTaskTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Security.Policy;
using FluentAssertions;
using Microsoft.OData.UriParser;
using Nest;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Helpers;

namespace OnlineSales.Tests;

public class DomainTaskTests : BaseTest
{   
    private readonly string tasksUrl = "/api/tasks";

    private readonly string domainsUrl = "/api/domains";

    private readonly string taskName = "DomainVerificationTask";

    [Fact]
    public async Task ExecuteTest()
    {
        await TryToStop();

        var validDomain = new TestDomain()
        {
            Name = "gmail.com",
        };

        var invalidDomain = new TestDomain()
        {
            Name = "SomeIncorrectDomainName",
        };

        var filledDomain = new TestDomain()
        {
            Name = "filledDomainName",
            HttpCheck = false,
            Url = "Url",
            Title = "Title",
            Description = "Description",
            DnsRecords = new List<DnsRecord>(3),
            DnsCheck = false,
        };

        var validDomainLocation = await PostTest(domainsUrl, validDomain);

        var invalidDomainLocation = await PostTest(domainsUrl, invalidDomain);

        var filledDomainLocation = await PostTest(domainsUrl, filledDomain);

        var validDomainAdded = await GetTest<Domain>(validDomainLocation);
        validDomainAdded.Should().NotBeNull();   
        validDomainAdded!.HttpCheck.Should().BeNull();
        validDomainAdded!.DnsCheck.Should().BeNull();

        var invalidDomainAdded = await GetTest<Domain>(invalidDomainLocation);
        invalidDomainAdded.Should().NotBeNull();
        invalidDomainAdded!.HttpCheck.Should().BeNull();
        invalidDomainAdded!.DnsCheck.Should().BeNull();

        var filledDomainAdded = await GetTest<Domain>(filledDomainLocation);
        filledDomainAdded.Should().BeEquivalentTo(filledDomain);

        await TryToExecute();

        validDomainAdded = await GetTest<Domain>(validDomainLocation);
        validDomainAdded.Should().NotBeNull();
        validDomainAdded!.HttpCheck.Should().BeTrue();
        validDomainAdded!.Url.Should().NotBeNull();
        validDomainAdded!.Title.Should().NotBeNull();
        validDomainAdded!.Description.Should().NotBeNull();
        validDomainAdded!.DnsCheck.Should().BeTrue();
        validDomainAdded!.DnsRecords.Should().NotBeNull();

        invalidDomainAdded = await GetTest<Domain>(invalidDomainLocation);
        invalidDomainAdded.Should().NotBeNull();
        invalidDomainAdded!.HttpCheck.Should().BeFalse();
        invalidDomainAdded!.Url.Should().BeNull();
        invalidDomainAdded!.Title.Should().BeNull();
        invalidDomainAdded!.Description.Should().BeNull();
        invalidDomainAdded!.DnsCheck.Should().BeFalse();
        invalidDomainAdded!.DnsRecords.Should().BeNull();

        filledDomainAdded = await GetTest<Domain>(filledDomainLocation);
        filledDomainAdded.Should().BeEquivalentTo(filledDomain);
    }

    private async Task TryToStop()
    {
        var maxTries = 10;
        HttpResponseMessage responce = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        for (int i = 0; i < maxTries && responce!.StatusCode != HttpStatusCode.OK; ++i)
        {
            responce = await GetRequest(tasksUrl + "/stop/" + taskName);
        }

        responce.StatusCode.Should().Be(HttpStatusCode.OK);
        responce.Should().NotBeNull();

        var content = await responce.Content.ReadAsStringAsync();
        var task = JsonHelper.Deserialize<TaskDetailsDto>(content);

        task!.IsRunning.Should().BeFalse();
    }

    private async Task TryToExecute()
    {
        var maxTries = 10;

        HttpResponseMessage executeResponce = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        for (int i = 0; i < maxTries && executeResponce.StatusCode != HttpStatusCode.OK; ++i)
        {
            executeResponce = await GetRequest(tasksUrl + "/execute/" + taskName);
        }

        executeResponce.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await executeResponce.Content.ReadAsStringAsync();
        var taskStatus = JsonHelper.Deserialize<TaskExecutionDto>(content);

        taskStatus.Should().NotBeNull();
        taskStatus!.Name.Should().Be(taskName);
        taskStatus!.Completed.Should().BeTrue();
    }
}