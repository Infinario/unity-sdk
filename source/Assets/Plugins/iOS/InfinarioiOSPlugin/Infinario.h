//
//  Infinario.h
//  InfinarioSDK
//
//  Created by Igi on 2/4/15.
//  Copyright (c) 2015 Infinario. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <StoreKit/StoreKit.h>

@interface Infinario : NSObject
<SKPaymentTransactionObserver, SKProductsRequestDelegate>

+ (id)sharedInstanceWithToken:(NSString *)token andWithTarget:(NSString *)target andWithCustomerDict:(NSMutableDictionary *)customer;
+ (id)sharedInstanceWithToken:(NSString *)token andWithTarget:(NSString *)target andWithCustomer:(NSString *)customer;
+ (id)sharedInstanceWithToken:(NSString *)token andWithTarget:(NSString *)target;
+ (id)sharedInstanceWithToken:(NSString *)token andWithCustomerDict:(NSMutableDictionary *)customer;
+ (id)sharedInstanceWithToken:(NSString *)token andWithCustomer:(NSString *)customer;
+ (id)sharedInstanceWithToken:(NSString *)token;

- (void)identifyWithCustomerDict:(NSMutableDictionary *)customer andUpdate:(NSDictionary *)properties;
- (void)identifyWithCustomer:(NSString *)customer andUpdate:(NSDictionary *)properties;
- (void)identifyWithCustomerDict:(NSMutableDictionary *)customer;
- (void)identifyWithCustomer:(NSString *)customer;

- (void)update:(NSDictionary *)properties;

- (void)track:(NSString *)type withProperties:(NSDictionary *)properties withTimestamp:(NSNumber *)timestamp;
- (void)track:(NSString *)type withProperties:(NSDictionary *)properties;
- (void)track:(NSString *)type withTimestamp:(NSNumber *)timestamp;
- (void)track:(NSString *)type;
- (void)trackVirtualPayment:(NSString *)currency withAmount:(NSNumber *)amount withItemName:(NSString *)itemName withItemType:(NSString *)itemType;

- (void)setSessionProperties:(NSDictionary *)properties;

- (void)enableAutomaticFlushing;
- (void)disableAutomaticFlushing;
- (void)flush;

- (void)registerPushNotifications;
- (void)addPushNotificationsToken:(NSString *)token;

@end
