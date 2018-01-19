import { CommandResult, CQRS } from "@leancode/cqrs-client/CQRS";


export default class Client {
    constructor(private cqrsClient: CQRS) { }

    createNewBrandBanner = this.cqrsClient.executeCommand.bind(this.cqrsClient, "Appetiq.Core.Contracts.BrandManager.Banners.Commands.CreateNewBrandBanner") as (dto: CreateNewBrandBanner) => Promise<CommandResult>;
    deleteBrandBanner = this.cqrsClient.executeCommand.bind(this.cqrsClient, "Appetiq.Core.Contracts.BrandManager.Banners.Commands.DeleteBrandBanner") as (dto: DeleteBrandBanner) => Promise<CommandResult>;
    updateBrandBanner = this.cqrsClient.executeCommand.bind(this.cqrsClient, "Appetiq.Core.Contracts.BrandManager.Banners.Commands.UpdateBrandBanner") as (dto: UpdateBrandBanner) => Promise<CommandResult>;
    allBrandBanners = this.cqrsClient.executeQuery.bind(this.cqrsClient, "Appetiq.Core.Contracts.BrandManager.Banners.Queries.AllBrandBanners") as (dto: AllBrandBanners) => Promise<BrandBannerDTO[]>;
    allTags = this.cqrsClient.executeQuery.bind(this.cqrsClient, "Appetiq.Core.Contracts.BrandManager.Banners.Queries.AllTags") as (dto: AllTags) => Promise<TagDTO[]>;
    exportOrdersToCsv = this.cqrsClient.executeQuery.bind(this.cqrsClient, "Appetiq.Core.Contracts.BrandManager.ExportOrdersToCsv") as (dto: ExportOrdersToCsv) => Promise<OrdersCsvExportDTO>;
    tokenForBrandContainer = this.cqrsClient.executeQuery.bind(this.cqrsClient, "Appetiq.Core.Contracts.BrandManager.Imaging.TokenForBrandContainer") as (dto: TokenForBrandContainer) => Promise<AzureTokenDTO>;
    updateBrandBannerLearnMorePhotoMobile = this.cqrsClient.executeCommand.bind(this.cqrsClient, "Appetiq.Core.Contracts.BrandManager.Imaging.UpdateBrandBannerLearnMorePhotoMobile") as (dto: UpdateBrandBannerLearnMorePhotoMobile) => Promise<CommandResult>;
    updateBrandBannerLearnMorePhotoWeb = this.cqrsClient.executeCommand.bind(this.cqrsClient, "Appetiq.Core.Contracts.BrandManager.Imaging.UpdateBrandBannerLearnMorePhotoWeb") as (dto: UpdateBrandBannerLearnMorePhotoWeb) => Promise<CommandResult>;
    updateBrandBannerListPhotoMobile = this.cqrsClient.executeCommand.bind(this.cqrsClient, "Appetiq.Core.Contracts.BrandManager.Imaging.UpdateBrandBannerListPhotoMobile") as (dto: UpdateBrandBannerListPhotoMobile) => Promise<CommandResult>;
    updateBrandBannerListPhotoWeb = this.cqrsClient.executeCommand.bind(this.cqrsClient, "Appetiq.Core.Contracts.BrandManager.Imaging.UpdateBrandBannerListPhotoWeb") as (dto: UpdateBrandBannerListPhotoWeb) => Promise<CommandResult>;
    updateBrandBannerMainPhotoMobile = this.cqrsClient.executeCommand.bind(this.cqrsClient, "Appetiq.Core.Contracts.BrandManager.Imaging.UpdateBrandBannerMainPhotoMobile") as (dto: UpdateBrandBannerMainPhotoMobile) => Promise<CommandResult>;
    createMerchant = this.cqrsClient.executeCommand.bind(this.cqrsClient, "Appetiq.Core.Contracts.BrandManager.Merchants.CreateMerchant") as (dto: CreateMerchant) => Promise<CommandResult>;
    allMerchants = this.cqrsClient.executeQuery.bind(this.cqrsClient, "Appetiq.Core.Contracts.BrandManager.Merchants.AllMerchants") as (dto: AllMerchants) => Promise<MerchantDTO[]>;
    merchantDetails = this.cqrsClient.executeQuery.bind(this.cqrsClient, "Appetiq.Core.Contracts.BrandManager.Merchants.MerchantDetails") as (dto: MerchantDetails) => Promise<MerchantDetailsDTO>;
    allOrders = this.cqrsClient.executeQuery.bind(this.cqrsClient, "Appetiq.Core.Contracts.BrandManager.Orders.AllOrders") as (dto: AllOrders) => Promise<OrderDTO[]>;
    orderDetails = this.cqrsClient.executeQuery.bind(this.cqrsClient, "Appetiq.Core.Contracts.BrandManager.OrderDetails") as (dto: OrderDetails) => Promise<OrderDetailsDTO>;
    ordersReport = this.cqrsClient.executeQuery.bind(this.cqrsClient, "Appetiq.Core.Contracts.BrandManager.Orders.OrdersReport") as (dto: OrdersReport) => Promise<OrdersReportDTO>;
    allUsers = this.cqrsClient.executeQuery.bind(this.cqrsClient, "Appetiq.Core.Contracts.BrandManager.Users.AllUsers") as (dto: AllUsers) => Promise<UserDTO[]>;
    userDetails = this.cqrsClient.executeQuery.bind(this.cqrsClient, "Appetiq.Core.Contracts.BrandManager.Users.UserDetails") as (dto: UserDetails) => Promise<UserDetailsDTO>;
}
