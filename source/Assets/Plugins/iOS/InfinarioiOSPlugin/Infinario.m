//
//  Infinario.m
//  InfinarioSDK
//
//  Created by Igi on 2/4/15.
//  Copyright (c) 2015 Infinario. All rights reserved.
//

#import "Infinario.h"
#import "Preferences.h"
#import "Customer.h"
#import "Event.h"
#import "CommandManager.h"
#import "Http.h"
#import "Session.h"
#import "Device.h"
#import <AdSupport/ASIdentifierManager.h>

int const FLUSH_COUNT = 50;
double const FLUSH_DELAY = 10.0;

@interface Infinario ()

@property NSString *token;
@property NSString *target;
@property NSMutableDictionary *customer;
@property CommandManager *commandManager;
@property Preferences *preferences;
@property int commandCounter;
@property (nonatomic) BOOL automaticFlushing;
@property NSTimer *flushTimer;
@property UIBackgroundTaskIdentifier task;
@property Session *session;
@property NSDictionary *customSessionProperties;
@property NSString *receipt64;

@end

@implementation Infinario

- (instancetype)initWithToken:(NSString *)token andWithTarget:(NSString *)target andWithCustomer:(NSMutableDictionary *)customer {
    self = [super init];
    
    self.token = token;
    self.target = target;
    
    self.commandManager = [[CommandManager alloc] initWithTarget:self.target andWithToken:self.token];
    self.preferences = [Preferences sharedInstance];
    
    self.customer = nil;
    self.session = nil;
    self.commandCounter = FLUSH_COUNT;
    self.task = UIBackgroundTaskInvalid;
    self.sessionProperties = @{};
    
    _automaticFlushing = [[self.preferences objectForKey:@"automatic_flushing" withDefault:@YES] boolValue];
    
    [[SKPaymentQueue defaultQueue] addTransactionObserver:self];
    
    if (!customer){
        customer = [NSMutableDictionary dictionary];
    }
    
    self.customer = customer;
    [self setupSession];
    
    if ([[self getAppleAdvertisingId ] isEqualToString:@""]){
        [self initializeAppleAdvertisingId];
    }
    
    return self;
}

+ (id)sharedInstanceWithToken:(NSString *)token andWithTarget:(NSString *)target andWithCustomerDict:(NSMutableDictionary *)customer {
    static dispatch_once_t p = 0;
    
    __strong static id _sharedObject = nil;
    
    dispatch_once(&p, ^{
        _sharedObject = [[self alloc] initWithToken:token andWithTarget:target andWithCustomer:customer];
    });
    
    return _sharedObject;
}

+ (id)sharedInstanceWithToken:(NSString *)token andWithTarget:(NSString *)target andWithCustomer:(NSString *)customer {
    return [self sharedInstanceWithToken:token andWithTarget:target andWithCustomerDict:[self customerDict:customer]];
}

+ (id)sharedInstanceWithToken:(NSString *)token andWithTarget:(NSString *)target {
    return [self sharedInstanceWithToken:token andWithTarget:target andWithCustomerDict:nil];
}

+ (id)sharedInstanceWithToken:(NSString *)token andWithCustomerDict:(NSMutableDictionary *)customer {
    return [self sharedInstanceWithToken:token andWithTarget:nil andWithCustomerDict:customer];
}

+ (id)sharedInstanceWithToken:(NSString *)token andWithCustomer:(NSString *)customer {
    return [self sharedInstanceWithToken:token andWithTarget:nil andWithCustomerDict:[self customerDict:customer]];
}

+ (id)sharedInstanceWithToken:(NSString *)token {
    return [self sharedInstanceWithToken:token andWithTarget:nil andWithCustomerDict:nil];
}

+ (NSMutableDictionary *)customerDict:(NSString *)customer {
    NSMutableDictionary *dict = [[NSMutableDictionary alloc] init];
    
    if (customer) {
        dict[@"registered"] = customer;
    }
    
    return dict;
}

- (void)identifyWithCustomerDict:(NSMutableDictionary *)customer andUpdate:(NSDictionary *)properties {
    if (customer[@"registered"] && ![customer[@"registered"] isEqualToString:@""]) {
        NSMutableDictionary *identificationProperties = [Device deviceProperties];
        identificationProperties[@"registered"] = customer[@"registered"];
        [self.customer setObject:customer[@"registered"] forKey:@"registered"];
        
        [self track:@"identification" withProperties:identificationProperties];
        
        if (properties) [self update:properties];
    }
}

- (void)identifyWithCustomer:(NSString *)customer andUpdate:(NSDictionary *)properties {
    [self identifyWithCustomerDict:[[self class] customerDict:customer] andUpdate:properties];
}

- (void)identifyWithCustomerDict:(NSMutableDictionary *)customer {
    [self identifyWithCustomerDict:customer andUpdate:nil];
}

- (void)identifyWithCustomer:(NSString *)customer {
    [self identifyWithCustomer:customer andUpdate:nil];
}

- (void)unidentify {
    [self.preferences removeObjectForKey:@"cookie"];
    self.customer = nil;
}

- (void)update:(NSDictionary *)properties {
    Customer *customer = [[Customer alloc] initWithIds:self.customer andProjectId:self.token andWithProperties:properties];
    
    [self.commandManager schedule:customer];
    
    if (self.automaticFlushing) [self setupDelayedFlush];
}

- (void)track:(NSString *)type withProperties:(NSDictionary *)properties withTimestamp:(NSNumber *)timestamp {
    Event *event = [[Event alloc] initWithIds:self.customer andProjectId:self.token andWithType:type andWithProperties:properties andWithTimestamp:timestamp];
    
    [self.commandManager schedule:event];
    
    if (self.automaticFlushing) [self setupDelayedFlush];
}

- (void)track:(NSString *)type withProperties:(NSDictionary *)properties {
    [self track:type withProperties:properties withTimestamp:nil];
}

- (void)track:(NSString *)type withTimestamp:(NSNumber *)timestamp {
    [self track:type withProperties:nil withTimestamp:timestamp];
}

- (void)track:(NSString *)type {
    [self track:type withProperties:nil withTimestamp:nil];
}

- (void)trackVirtualPayment:(NSString *)currency withAmount:(NSNumber *)amount withItemName:(NSString *)itemName withItemType:(NSString *)itemType{
    NSMutableDictionary *virtualPayment = [Device deviceProperties];
    
    [virtualPayment setObject:currency forKey:@"currency"];
    [virtualPayment setObject:amount forKey:@"amount"];
    [virtualPayment setObject:itemName forKey:@"item_name"];
    [virtualPayment setObject:itemType forKey:@"item_type"];
    
    [self track:@"virtual_payment" withProperties:virtualPayment];
}

- (void)trackLogDebug:(NSString *)tag withMessage:(NSString *)message{
    [self trackLog:@"log_debug" withTag:tag withMessage:message withProperties:nil];
}

- (void)trackLogDebug:(NSString *)tag withMessage:(NSString *)message withProperties:(NSDictionary *)properties{
    [self trackLog:@"log_debug" withTag:tag withMessage:message withProperties:properties];
}

- (void)trackLogWarning:(NSString *)tag withMessage:(NSString *)message{
    [self trackLog:@"log_warning" withTag:tag withMessage:message withProperties:nil];
}

- (void)trackLogWarning:(NSString *)tag withMessage:(NSString *)message withProperties:(NSDictionary *)properties{
    [self trackLog:@"log_warning" withTag:tag withMessage:message withProperties:properties];
}

- (void)trackLogError:(NSString *)tag withMessage:(NSString *)message{
    [self trackLog:@"log_error" withTag:tag withMessage:message withProperties:nil];
}

- (void)trackLogError:(NSString *)tag withMessage:(NSString *)message withProperties:(NSDictionary *)properties{
    [self trackLog:@"log_error" withTag:tag withMessage:message withProperties:properties];
}

- (void)trackLog:(NSString *)type withTag:(NSString *)tag withMessage:(NSString *)message withProperties:(NSDictionary *)properties{
    NSMutableDictionary *logMessage = [[NSMutableDictionary alloc] init];
    [logMessage setObject:tag forKey:@"tag"];
    [logMessage setObject:message forKey:@"message"];
    if (properties) {
        [logMessage addEntriesFromDictionary:properties];
    }
    
    [self track:type withProperties:logMessage withTimestamp:nil];
}

- (void)setSessionProperties:(NSDictionary *)properties {
    self.customSessionProperties = properties;
}

- (void)flush {
    [self ensureBackgroundTask];
    
    dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
        @synchronized(self.commandManager) {
            [self.commandManager flush];
            [self ensureBackgroundTaskFinished];
        }
    });
}

- (void)ensureBackgroundTask {
    UIApplication *app = [UIApplication sharedApplication];
    
    if (self.task == UIBackgroundTaskInvalid) {
        self.task = [app beginBackgroundTaskWithExpirationHandler:^{
            [app endBackgroundTask:self.task];
            self.task = UIBackgroundTaskInvalid;
        }];
    }
}

- (void)ensureBackgroundTaskFinished {
    if (self.task != UIBackgroundTaskInvalid) {
        [[UIApplication sharedApplication] endBackgroundTask:self.task];
        self.task = UIBackgroundTaskInvalid;
    }
}

- (NSString *)getCookie {
    return [self.preferences objectForKey:@"cookie" withDefault:@""];
}

- (void)setupDelayedFlush {
    if (self.commandCounter > 0) {
        self.commandCounter--;
        [self startFlushTimer];
    }
    else {
        self.commandCounter = FLUSH_COUNT;
        [self stopFlushTimer];
        [self flush];
    }
}

- (void)setAutomaticFlushing:(BOOL)automaticFlushing {
    [self.preferences setObject:[NSNumber numberWithBool:automaticFlushing] forKey:@"automatic_flushing"];
    _automaticFlushing = automaticFlushing;
}

- (void)enableAutomaticFlushing {
    self.automaticFlushing = YES;
}

- (void)disableAutomaticFlushing {
    self.automaticFlushing = NO;
}

- (void)startFlushTimer {
    [self stopFlushTimer];
    [self ensureBackgroundTask];
    
    self.flushTimer = [NSTimer scheduledTimerWithTimeInterval:FLUSH_DELAY target:self selector:@selector(onFlushTimer:) userInfo:nil repeats:NO];
}

- (void)stopFlushTimer {
    if (self.flushTimer) {
        [self.flushTimer invalidate];
        self.flushTimer = nil;
    }
}

- (void)onFlushTimer:(NSTimer *)timer {
    if (self.automaticFlushing) [self flush];
    
    [self ensureBackgroundTaskFinished];
}

- (void)registerPushNotifications {
    UIUserNotificationType types = UIUserNotificationTypeAlert | UIUserNotificationTypeBadge | UIUserNotificationTypeSound;
    UIUserNotificationSettings *settings = [UIUserNotificationSettings settingsForTypes:types categories:nil];
    [[UIApplication sharedApplication] registerUserNotificationSettings:settings];
    [[UIApplication sharedApplication] registerForRemoteNotifications];
}

- (void)addPushNotificationsToken:(NSString *)token {
    [self update:@{@"apple_push_notification_id": token}];
}

- (void)setupSession {
    self.session = [[Session alloc] initWithPreferences:self.preferences];
    
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(sessionStart:) name:@"SessionStart" object:self.session];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(sessionEnd:) name:@"SessionEnd" object:self.session];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(sessionRestart:) name:@"SessionRestart" object:self.session];
    
    [self.session run];
}

- (void)sessionStart:(NSNotification *) notification {
    NSMutableDictionary *properties = [Device deviceProperties];
    [properties addEntriesFromDictionary:self.customSessionProperties];
    
    NSString *appVersion = [[[NSBundle mainBundle] infoDictionary] objectForKey:@"CFBundleShortVersionString"];
    if (appVersion){
        [properties setObject:appVersion forKey:@"app_version"];
    }
    
    [self track:@"session_start" withProperties:properties withTimestamp:notification.userInfo[@"timestamp"]];
}

- (void)sessionEnd:(NSNotification *) notification {
    NSMutableDictionary *properties = [Device deviceProperties];
    properties[@"duration"] = notification.userInfo[@"duration"];
    [properties addEntriesFromDictionary:self.customSessionProperties];
    
    NSString *appVersion = [[[NSBundle mainBundle] infoDictionary] objectForKey:@"CFBundleShortVersionString"];
    if (appVersion){
        [properties setObject:appVersion forKey:@"app_version"];
    }
    
    [self track:@"session_end" withProperties:properties withTimestamp:notification.userInfo[@"timestamp"]];
}

- (void)sessionRestart:(NSNotification *) notification {
    self.customer = notification.userInfo[@"customer"];
}

- (void)productsRequest:(SKProductsRequest *)request didReceiveResponse:(SKProductsResponse *)response {
    for (SKProduct *product in response.products) {
        //NSLog(@"Infinario: tracking hard purchase %@", [product productIdentifier]);
        
        NSMutableDictionary *properties = [Device deviceProperties];
        
        properties[@"gross_amount"] = product.price;
        properties[@"currency"] = [product.priceLocale objectForKey:NSLocaleCurrencyCode];
        properties[@"product_id"] = product.productIdentifier;
        properties[@"product_title"] = product.localizedTitle;
        properties[@"payment_system"] = @"iTunes Store";
        properties[@"receipt"] = self.receipt64;
        
        [self track:@"payment" withProperties:properties];
    }
}

- (void)paymentQueue:(SKPaymentQueue *)queue updatedTransactions:(NSArray *)transactions {
    NSMutableSet *products = [NSMutableSet setWithCapacity:transactions.count];
    
    for (SKPaymentTransaction *transaction in transactions) {
        switch (transaction.transactionState) {
            case SKPaymentTransactionStatePurchased:
                //NSLog(@"Infinario: an item has been bought: %@", [[transaction payment] productIdentifier]);
                [products addObject:[[transaction payment] productIdentifier]];
                self.receipt64 = transaction.transactionReceipt.base64Encoding;
                [[SKPaymentQueue defaultQueue] finishTransaction:transaction];
                break;
                
            case SKPaymentTransactionStateFailed:
                [[SKPaymentQueue defaultQueue] finishTransaction:transaction];
                break;
                
            default:
                break;
        }
    }
    
    if (products.count > 0 && [SKPaymentQueue canMakePayments]) {
        SKProductsRequest *request = [[SKProductsRequest alloc] initWithProductIdentifiers:products];
        request.delegate = self;
        [request start];
    }
}

- (NSString *)getAppleAdvertisingId{
    return [self.preferences objectForKey:@"apple_advertising_id" withDefault:@""];
}

- (void)initializeAppleAdvertisingId{
    NSString *advertisingId =[[ASIdentifierManager sharedManager].advertisingIdentifier UUIDString];
    
    [self.preferences setObject:advertisingId forKey:@"apple_advertising_id"];
    
    NSMutableDictionary *properties = [NSMutableDictionary dictionary];
    [properties setObject:advertisingId forKey:@"apple_advertising_id"];
    
    [self update:properties];
}

@end
