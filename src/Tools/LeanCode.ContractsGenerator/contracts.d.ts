declare interface AuthorizeWhenAttribute {}declare namespace contracts {
    interface BrandContext {
        BrandId: string;
        DoerId: string;
    }

    interface BrandQuery<TResult> extends IRemoteQuery<BrandContext, TResult> {
    }

    interface BrandCommand extends IRemoteCommand<BrandContext> {
    }

    interface Permissions {
    }

    interface AuthorizeWhenBrandOwnsBanner extends AuthorizeWhenAttribute {
    }

    interface BrandOwnsBanner {
    }

    interface IBannerRelated {
        BannerId: string;
    }

    interface AuthorizeWhenBrandOwnsMerchantForBanner extends AuthorizeWhenAttribute {
    }

    interface BrandOwnsMerchantForBanner {
    }

    interface IMerchantBannerRelated {
        MerchantId?: string;
    }

    interface BrandBannerDTO {
        Id: string;
        BrandId: string;
        MainPhotoMobile: string;
        ListPhotoMobile: string;
        LearnMorePhotoMobile: string;
        ListPhotoWeb: string;
        LearnMorePhotoWeb: string;
        OriginalMainPhotoMobileId?: string;
        OriginalListPhotoMobileId?: string;
        OriginalLearnMorePhotoMobileId?: string;
        OriginalListPhotoWebId?: string;
        OriginalLearnMorePhotoWebId?: string;
        Title: string;
        HTMLContent: string;
        CTALabel: string;
        StartDate: string;
        EndDate: string;
        Type?: BrandBannerTypeDTO;
        DishId?: string;
        MenuId: string;
        MerchantId?: string;
        InternalTags: string[];
        IsLearnMoreViewEnabled: boolean;
        IsLinkingToContent: boolean;
        IsFullScreenEnabled: boolean;
    }

    enum BrandBannerTypeDTO {
        Menu = 0,
        Dish = 1,
        SingleMerchant = 2,
        MerchantsList = 3,
        AllMerchants = 4
    }

    interface TagDTO {
        Id: string;
        Name: string;
    }

    interface CreateNewBrandBanner extends IMerchantBannerRelated, BrandCommand {
        Id: string;
        MainPhotoMobile: string;
        ListPhotoMobile: string;
        LearnMorePhotoMobile: string;
        ListPhotoWeb: string;
        LearnMorePhotoWeb: string;
        Title: string;
        HTMLContent: string;
        CTALabel: string;
        StartDate: string;
        EndDate: string;
        Type?: BrandBannerTypeDTO;
        DishId?: string;
        MenuId: string;
        MerchantId?: string;
        Tags: string[];
        IsLearnMoreViewEnabled: boolean;
        IsLinkingToContent: boolean;
        IsFullScreenEnabled: boolean;
    }

    interface DeleteBrandBanner extends IBannerRelated, BrandCommand {
        BannerId: string;
    }

    interface UpdateBrandBanner extends IBannerRelated, IMerchantBannerRelated, BrandCommand {
        BannerId: string;
        Title: string;
        HTMLContent: string;
        CTALabel: string;
        StartDate: string;
        EndDate: string;
        Type?: BrandBannerTypeDTO;
        DishId?: string;
        MenuId: string;
        MerchantId?: string;
        Tags: string[];
        IsLearnMoreViewEnabled: boolean;
        IsLinkingToContent: boolean;
        IsFullScreenEnabled: boolean;
    }

    interface AllBrandBanners extends BrandQuery<BrandBannerDTO[]> {
    }

    interface AllTags extends BrandQuery<TagDTO[]> {
    }

    interface ExportOrdersToCsv extends IOrdersReport, BrandQuery<OrdersCsvExportDTO> {
        MerchantId: string;
        CreatedAfter: string;
        CreatedBefore: string;
    }

    interface ICsvRelated {
        CsvData: string;
    }

    interface OrdersCsvExportDTO {
        FileName: string;
        FileContent: string;
    }

    interface AzureTokenDTO {
        ContainerUri: string;
        Token: string;
        Container: string;
    }

    interface TokenForBrandContainer extends BrandQuery<AzureTokenDTO> {
    }

    interface UpdateBrandBannerLearnMorePhotoMobile extends IBannerRelated, UpdateImage<UpdateBrandBannerLearnMorePhotoMobile> {
        BannerId: string;
    }

    interface UpdateBrandBannerLearnMorePhotoWeb extends IBannerRelated, UpdateImage<UpdateBrandBannerLearnMorePhotoWeb> {
        BannerId: string;
    }

    interface UpdateBrandBannerListPhotoMobile extends IBannerRelated, UpdateImage<UpdateBrandBannerListPhotoMobile> {
        BannerId: string;
    }

    interface UpdateBrandBannerListPhotoWeb extends IBannerRelated, UpdateImage<UpdateBrandBannerListPhotoWeb> {
        BannerId: string;
    }

    interface UpdateBrandBannerMainPhotoMobile extends IBannerRelated, UpdateImage<UpdateBrandBannerMainPhotoMobile> {
        BannerId: string;
    }

    interface ImageSizeDTO {
        Width: number;
        Height: number;
    }

    interface ImageSourceDTO {
        Top: number;
        Left: number;
        Width: number;
        Height: number;
    }

    interface UpdateImage<TCommand extends UpdateImage<TCommand>> extends BrandCommand {
        ImageId: string;
        TargetSize: ImageSizeDTO;
        SourceSelection: ImageSourceDTO;
        Rotation: number;
        Quality: number;
    }

    interface MerchantDetailsDTO extends MerchantDTO {
        Currency: string;
    }

    interface MerchantDTO {
        Id: string;
        Name: string;
        DateCreated: string;
        Address: string;
        IsOpen: boolean;
    }

    interface CreateMerchant extends BrandCommand {
        Id: string;
        Name: string;
        Currency: string;
    }

    interface AllMerchants extends BrandQuery<MerchantDTO[]> {
    }

    interface MerchantDetails extends BrandQuery<MerchantDetailsDTO> {
        MerchantId: string;
    }

    interface DeliveryPlaceDTO {
        Type: DeliveryPlaceTypeDTO;
        CheckinName: string;
        AddressId?: string;
        Address: UserAddressDTO;
        CheckinType: number;
    }

    enum DeliveryPlaceTypeDTO {
        CheckinPoint = 0,
        Address = 1
    }

    interface OrderAdditionalCostDTO {
        Id: string;
        RelatedItemId?: string;
        Type: OrderAdditionalCostTypeDTO;
        Price: number;
    }

    enum OrderAdditionalCostTypeDTO {
        Packaging = 0,
        Delivery = 1
    }

    interface OrderDetailsDTO extends OrderDTO {
        DateClosed?: string;
        DateConfirmed?: string;
        PaymentMethod: PaymentMethodDTO;
        Items: OrderItemDTO[];
        Discounts: OrderDiscountDTO[];
        AdditionalCosts: OrderAdditionalCostDTO[];
    }

    interface OrderDiscountDTO {
        Id: string;
        Description: string;
        Value: number;
        Type: OrderDiscountTypeDTO;
    }

    enum OrderDiscountTypeDTO {
        Appetiq = 0,
        Restaurant = 1
    }

    interface OrderDTO {
        Id: string;
        Number: number;
        Status: OrderStatusDTO;
        Menu: string;
        DeliveryPlace: DeliveryPlaceDTO;
        TotalPrice: number;
        PriceForMerchant: number;
        TotalPriceWithoutDiscounts: number;
        Currency: string;
        DateCreated: string;
        DateAccepted?: string;
        EstimatedDeliveryTimeInMinutes?: number;
        EstimatedDeliveryTime?: string;
        UserId: string;
        UserName: string;
        MerchantId: string;
        MerchantName: string;
    }

    interface OrderItemDTO {
        Id: string;
        DishId: string;
        Price: number;
        BasePrice: number;
        DishName: string;
        DishFullName: string;
        IsDelivered: boolean;
        ProcessingStarted: boolean;
        Options: OrderItemOptionDTO[];
    }

    interface OrderItemOptionDTO {
        OptionSetId: string;
        OptionSetName: string;
        OptionId: string;
        OptionName: string;
        IsOptionDefault: boolean;
        PriceModifier: number;
        AdditionalData: string;
    }

    interface OrdersReportDTO {
        MerchantName: string;
        From: string;
        To: string;
        Commission: number;
        CommissionRate: string;
        TransactionCost: number;
        TransactionRate: string;
        SubscriptionCost: number;
        NetValue: number;
        TaxValue: number;
        GrossValue: number;
        ToPay: number;
        OrderValueSum: number;
        Sum: number;
        TotalSum: number;
        Currency: string;
        Orders: OrderDTO[];
    }

    enum OrderStatusDTO {
        New = 0,
        Canceled = 1,
        WaitingForPayment = 2,
        Confirmed = 3,
        Accepted = 4,
        Deleted = 5,
        Closed = 6
    }

    enum PaymentMethodDTO {
        NotSet = 0,
        Cash = 1,
        BraintreeCreditCard = 11,
        BraintreePayPal = 12,
        PayUTouch = 20,
        PayU = 21,
        Stripe = 30,
        InstantPath = 100
    }

    interface AllOrders extends BrandQuery<OrderDTO[]> {
        CreatedAfter: string;
        CreatedBefore: string;
    }

    interface IOrderRelatedQuery {
        OrderId: string;
    }

    interface IOrdersReport {
        MerchantId: string;
        CreatedAfter: string;
        CreatedBefore: string;
    }

    interface OrderDetails extends IOrderRelatedQuery, BrandQuery<OrderDetailsDTO> {
        OrderId: string;
    }

    interface OrdersReport extends IOrdersReport, BrandQuery<OrdersReportDTO> {
        MerchantId: string;
        CreatedAfter: string;
        CreatedBefore: string;
    }

    interface UserAddressDTO {
        Id: string;
        City: string;
        Street: string;
        Building: string;
        Floor: string;
        Apartment: string;
        Notes: string;
        DateCreated: string;
    }

    interface UserDetailsDTO extends UserDTO {
        RegistrationPlatform: string;
        Orders: OrderDTO[];
    }

    interface UserDTO {
        Id: string;
        FirstName: string;
        LastName: string;
        Email: string;
        PhoneNumber: string;
        DateRegistered: string;
    }

    interface AllUsers extends BrandQuery<UserDTO[]> {
        RegisteredAfter: string;
        RegisteredBefore: string;
    }

    interface UserDetails extends BrandQuery<UserDetailsDTO> {
        UserId: string;
    }

}
