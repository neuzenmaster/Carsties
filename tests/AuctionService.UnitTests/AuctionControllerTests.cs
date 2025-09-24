using System;
using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.RequestHelpers;
using AuctionService.UnitTests.Utils;
using AutoFixture;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuctionService.UnitTests;

public class AuctionControllerTests
{
    private readonly Mock<IAuctionRepository> _auctionRepository;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly IMapper _mapper;
    private readonly Fixture _fixture;
    private readonly AuctionsController _controller;

    public AuctionControllerTests()
    {
        _fixture = new Fixture();
        _auctionRepository = new Mock<IAuctionRepository>();
        _publishEndpoint = new Mock<IPublishEndpoint>();

        var mockMapper = new MapperConfiguration(mc =>
        {
            mc.AddMaps(typeof(MappingProfiles).Assembly);
        }).CreateMapper().ConfigurationProvider;

        _mapper = new Mapper(mockMapper);
        _controller = new AuctionsController(_auctionRepository.Object, _mapper, _publishEndpoint.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext { User = Helpers.GetClaimsPrincipal() }
            }
        };
    }

    [Fact]
    public async Task GetAuctions_WithNoParams_Returns10Auctions()
    {
        // arrange
        var auctions = _fixture.CreateMany<AuctionDto>(10).ToList();
        _auctionRepository.Setup(repo => repo.GetAuctionsAsync(null)).ReturnsAsync(auctions);

        // act
        var result = await _controller.GetAllAuctions(null);

        // assert
        Assert.Equal(10, result.Value.Count);
        Assert.IsType<ActionResult<List<AuctionDto>>>(result);
    }

    [Fact]
    public async Task GetAuctionById_WithValidGuid_ReturnsAuction()
    {
        // arrange
        var auction = _fixture.Create<AuctionDto>();
        _auctionRepository.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

        // act
        var result = await _controller.GetAuctionById(auction.Id);

        // assert
        Assert.Equal(auction.Make, result.Value.Make);
        Assert.IsType<ActionResult<AuctionDto>>(result);
    }

    [Fact]
    public async Task GetAuctionById_WithInValidGuid_ReturnsNotFound()
    {
        // arrange
        _auctionRepository.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);

        // act
        var result = await _controller.GetAuctionById(Guid.NewGuid());

        // assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateAuction_WithInValidAuction_ReturnsCreatedAction()
    {
        // arrange
        var auction = _fixture.Create<CreateAuctionDto>();
        _auctionRepository.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        _auctionRepository.Setup(repo => repo.SaveChangesAync()).ReturnsAsync(true);

        // act
        var result = await _controller.CreateAuction(auction);
        var createdAtResult = result.Result as CreatedAtActionResult;

        // assert
        Assert.NotNull(createdAtResult);
        Assert.Equal("GetAuctionById", createdAtResult.ActionName);
        Assert.IsType<AuctionDto>(createdAtResult.Value);
    }

    [Fact]
    public async Task CreateAuction_WithFailedSaveChangesAync_ReturnsNotFound()
    {
        // arrange
        var auction = _fixture.Create<CreateAuctionDto>();
        _auctionRepository.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        _auctionRepository.Setup(repo => repo.SaveChangesAync()).ReturnsAsync(false);

        // act
        var result = await _controller.CreateAuction(auction);

        // assert        
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateAuction_WithValidGuidAndAuction_ReturnsOk()
    {
        // arrange
        var item = _fixture.Build<Item>().Without(a => a.Auction).Create();
        var auctionEntity = _fixture.Build<Auction>()
            .With(a => a.Seller, "test")
            .With(a => a.Item, item).Create();
        var auction = _fixture.Create<UpdateAuctionDto>();
        _auctionRepository.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auctionEntity);
        _auctionRepository.Setup(repo => repo.SaveChangesAync()).ReturnsAsync(true);

        // act
        var result = await _controller.UpdateAuction(Guid.NewGuid(), auction);

        // assert        
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithFailedSaveChangesAsync_ReturnsBadRequest()
    {
        // arrange
        var item = _fixture.Build<Item>().Without(a => a.Auction).Create();
        var auctionEntity = _fixture.Build<Auction>()
            .With(a => a.Seller, "test")
            .With(a => a.Item, item).Create();
        var auction = _fixture.Create<UpdateAuctionDto>();
        _auctionRepository.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auctionEntity);
        _auctionRepository.Setup(repo => repo.SaveChangesAync()).ReturnsAsync(false);

        // act
        var result = await _controller.UpdateAuction(Guid.NewGuid(), auction);

        // assert        
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithGetAuctionEntityByIdReturningNull_ReturnsNotFoundResult()
    {
        // arrange
        var item = _fixture.Build<Item>().Without(a => a.Auction).Create();
        var auctionEntity = _fixture.Build<Auction>()
            .With(a => a.Seller, "test")
            .With(a => a.Item, item).Create();
        var auction = _fixture.Create<UpdateAuctionDto>();
        _auctionRepository.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(value: null);

        // act
        var result = await _controller.UpdateAuction(Guid.NewGuid(), auction);

        // assert        
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithValidGuid_ReturnsOk()
    {
        // arrange
        var item = _fixture.Build<Item>().Without(a => a.Auction).Create();
        var auctionEntity = _fixture.Build<Auction>()
            .With(a => a.Seller, "test")
            .With(a => a.Item, item).Create();
        var auction = _fixture.Create<UpdateAuctionDto>();
        _auctionRepository.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auctionEntity);
        _auctionRepository.Setup(repo => repo.RemoveAuction(It.IsAny<Auction>()));
        _auctionRepository.Setup(repo => repo.SaveChangesAync()).ReturnsAsync(true);

        // act
        var result = await _controller.DeleteAuction(Guid.NewGuid());

        // assert        
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithGetAuctionEntityByIdReturnsNull_ReturnsNotFound()
    {
        // arrange
        var item = _fixture.Build<Item>().Without(a => a.Auction).Create();
        var auctionEntity = _fixture.Build<Auction>()
            .With(a => a.Seller, "test")
            .With(a => a.Item, item).Create();
        var auction = _fixture.Create<UpdateAuctionDto>();
        _auctionRepository.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(value: null);

        // act
        var result = await _controller.DeleteAuction(Guid.NewGuid());

        // assert        
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithSaveChangesAyncReturnsFalse_ReturnsBadRequestResult()
    {
        // arrange
        var item = _fixture.Build<Item>().Without(a => a.Auction).Create();
        var auctionEntity = _fixture.Build<Auction>()
            .With(a => a.Seller, "test")
            .With(a => a.Item, item).Create();
        var auction = _fixture.Create<UpdateAuctionDto>();
        _auctionRepository.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auctionEntity);
        _auctionRepository.Setup(repo => repo.RemoveAuction(It.IsAny<Auction>()));
        _auctionRepository.Setup(repo => repo.SaveChangesAync()).ReturnsAsync(false);

        // act
        var result = await _controller.DeleteAuction(Guid.NewGuid());

        // assert        
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
